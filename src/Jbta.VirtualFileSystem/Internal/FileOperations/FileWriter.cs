using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.SpaceManagement;
using Jbta.VirtualFileSystem.Internal.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileWriter
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly BlocksAllocator _blocksAllocator;
        private readonly BlocksDeallocator _blocksDeallocator;
        private readonly IBinarySerializer<FileMetaBlock> _fileMetaBlockSerializer;
        private readonly IVolumeReader _volumeReader;
        private readonly IVolumeWriter _volumeWriter;
        private readonly int _indirectBlockCapacity;
        private readonly int _maxFileDataSize;

        public FileWriter(
            FileSystemMeta fileSystemMeta,
            BlocksAllocator blocksAllocator,
            BlocksDeallocator blocksDeallocator,
            IBinarySerializer<FileMetaBlock> fileMetaBlockSerializer,
            IVolumeReader volumeReader,
            IVolumeWriter volumeWriter)
        {
            _fileSystemMeta = fileSystemMeta;
            _blocksAllocator = blocksAllocator;
            _blocksDeallocator = blocksDeallocator;
            _fileMetaBlockSerializer = fileMetaBlockSerializer;
            _volumeReader = volumeReader;
            _volumeWriter = volumeWriter;
            _indirectBlockCapacity = _fileSystemMeta.BlockSize / sizeof(int);
            _maxFileDataSize =
                (GlobalConstant.MaxFileDirectBlocksCount
                + GlobalConstant.MaxFileIndirectBlocksCount * _indirectBlockCapacity)
                * _fileSystemMeta.BlockSize;
        }

        public async Task Write(FileMetaBlock fileMetaBlock, int offsetInBytes, byte[] data)
        {
            if (offsetInBytes + data.Length > _maxFileDataSize)
            {
                throw new FileSystemException($"File cannot contain more, than {_maxFileDataSize} bytes");
            }

            var startBlockNumberInFile = offsetInBytes / _fileSystemMeta.BlockSize;

            IReadOnlyList<int> outdatedBlockNumbers;
            
            if (offsetInBytes % _fileSystemMeta.BlockSize == 0)
            {
                var blocksCount = data.Length.DivideWithUpRounding(_fileSystemMeta.BlockSize);
                outdatedBlockNumbers = await ReplaceBlocks(fileMetaBlock, data, blocksCount, startBlockNumberInFile);
            }
            else
            {
                // read start block
                var firstBlockNumber = await TranslateToBlockNumber(startBlockNumberInFile, fileMetaBlock);
                var startBlock = await _volumeReader.ReadBlocks(firstBlockNumber);
                
                // take head and concat it from the beginning to data array
                var h = offsetInBytes - startBlockNumberInFile * _fileSystemMeta.BlockSize;
                data = startBlock.Take(h).Concat(data).ToArray();
                
                var blocksCount = data.Length.DivideWithUpRounding(_fileSystemMeta.BlockSize);
                outdatedBlockNumbers = await ReplaceBlocks(fileMetaBlock, data, blocksCount, startBlockNumberInFile);
            }
            
            await SaveFileMetaBlock(fileMetaBlock);
            
            // deallocate outdated blocks
            await _blocksDeallocator.DeallocateBlocks(outdatedBlockNumbers);
        }

        private async Task<IReadOnlyList<int>> ReplaceBlocks(
            FileMetaBlock fileMetaBlock, byte[] blocksData, int addingBlocksCount, int startBlockNumberInFile)
        {
            var outdatedBlockNumbers = await CollectOutdatedBlocks(startBlockNumberInFile, fileMetaBlock);
            
            // write new data blocks
            blocksData = AdjustDataToBlockSize(blocksData, addingBlocksCount);
            var reservedBlocksNumbers = await _blocksAllocator.AllocateBlocks(addingBlocksCount);
            await _volumeWriter.WriteBlocks(blocksData, reservedBlocksNumbers);

            // add to direct blocks
            var freeDirectBlocksCount = GlobalConstant.MaxFileDirectBlocksCount - fileMetaBlock.DirectBlocksCount;
            if (freeDirectBlocksCount > 0)
            {
                foreach (var dataBlockNumber in reservedBlocksNumbers.Take(freeDirectBlocksCount))
                {
                    fileMetaBlock.DirectBlocks[fileMetaBlock.DirectBlocksCount] = dataBlockNumber;
                    fileMetaBlock.DirectBlocksCount++;
                }
            }

            // add to indirect blocks
            var blocksNumbersForIndirectPlacement = reservedBlocksNumbers.Skip(freeDirectBlocksCount).ToArray();
            if (blocksNumbersForIndirectPlacement.Any())
            {
                if (fileMetaBlock.IndirectBlocksCount > 0)
                {
                    // try to fil up free slots in last indirect block
                    blocksNumbersForIndirectPlacement =
                        await TryToFillUpLastIndirectBlock(fileMetaBlock, blocksNumbersForIndirectPlacement, outdatedBlockNumbers);
                    if (!blocksNumbersForIndirectPlacement.Any())
                    {
                        return outdatedBlockNumbers;
                    }
                }

                var indirectBlocksNumbers = await AddIndirectBlocks(blocksNumbersForIndirectPlacement);
                foreach (var dataBlockNumber in indirectBlocksNumbers)
                {
                    fileMetaBlock.IndirectBlocks[fileMetaBlock.IndirectBlocksCount] = dataBlockNumber;
                    fileMetaBlock.IndirectBlocksCount++;
                }
            }

            return outdatedBlockNumbers;
        }

        private async Task<int[]> TryToFillUpLastIndirectBlock(
            FileMetaBlock fileMetaBlock, int[] blocksNumbersForIndirectPlacement, IEnumerable<int> outdatedBlockNumbers)
        {
            var lastIndirectBlockNumber = fileMetaBlock.IndirectBlocks[fileMetaBlock.IndirectBlocksCount - 1];
            var indirectBlockData = await _volumeReader.ReadBlocks(lastIndirectBlockNumber);
            var lastIndirectBlockRefs = indirectBlockData.ToIntArray().Where(bn => bn != 0).Except(outdatedBlockNumbers).ToArray();
            var filledSlotsIntoIndirectBlockCount = lastIndirectBlockRefs.Length;
            if (filledSlotsIntoIndirectBlockCount >= _indirectBlockCapacity)
            {
                return blocksNumbersForIndirectPlacement;
            }
            
            var freeSlots = _indirectBlockCapacity - filledSlotsIntoIndirectBlockCount;
            lastIndirectBlockRefs =
                lastIndirectBlockRefs.Concat(blocksNumbersForIndirectPlacement.Take(freeSlots)).ToArray();
            var indirectBlocksData = AdjustDataToBlockSize(lastIndirectBlockRefs.ToByteArray(), 1);
            await _volumeWriter.WriteBlock(indirectBlocksData, lastIndirectBlockNumber);
            
            return blocksNumbersForIndirectPlacement.Skip(freeSlots).ToArray();
        }

        private async Task<IReadOnlyList<int>> AddIndirectBlocks(int[] blocksNumbers)
        {
            var countOfIndirectBlocks = blocksNumbers.Length.DivideWithUpRounding(_indirectBlockCapacity);
            var reservedIndirectBlocksNumbers = await _blocksAllocator.AllocateBlocks(countOfIndirectBlocks);

            var newIndirectBlocksData = AdjustDataToBlockSize(blocksNumbers.ToByteArray(), countOfIndirectBlocks);

            await _volumeWriter.WriteBlocks(newIndirectBlocksData, reservedIndirectBlocksNumbers);
            return reservedIndirectBlocksNumbers;
        }

        private byte[] AdjustDataToBlockSize(byte[] data, int countOfBlocks)
        {
            if (data.Length % _fileSystemMeta.BlockSize != 0)
            {
                Array.Resize(ref data, _fileSystemMeta.BlockSize * countOfBlocks);
            }
            return data;
        }
        
        private async Task<int> TranslateToBlockNumber(int blockNumberInFile, FileMetaBlock fileMetaBlock)
        {
            // from direct block
            if (blockNumberInFile < GlobalConstant.MaxFileDirectBlocksCount)
            {
                return fileMetaBlock.DirectBlocks[blockNumberInFile];
            }

            // from indirect block
            var blockNumberAmongIndirectPlacedBlocks = blockNumberInFile - GlobalConstant.MaxFileDirectBlocksCount;
            var indirectBlockIndex = blockNumberAmongIndirectPlacedBlocks / _indirectBlockCapacity;
            var indirectBlock = await _volumeReader.ReadBlocks(fileMetaBlock.IndirectBlocks[indirectBlockIndex]);
            
            return BitConverter.ToInt32(
                indirectBlock,
                blockNumberAmongIndirectPlacedBlocks * sizeof(int)
            );
        }

        private async Task<IReadOnlyList<int>> CollectOutdatedBlocks(int startBlockNumberInFile, FileMetaBlock fmb)
        {
            var result = new List<int>();
            
            // collect outdated blocks from direct block
            if (startBlockNumberInFile < fmb.DirectBlocksCount)
            {
                var collection = Enumerable
                    .Range(startBlockNumberInFile, fmb.DirectBlocksCount - startBlockNumberInFile)
                    .Select(directBlockNumber => fmb.DirectBlocks[startBlockNumberInFile]);
                result.AddRange(collection);
                
                fmb.DirectBlocksCount -= result.Count;    // shrink direct blocks
            }
            
            if (startBlockNumberInFile >= fmb.IndirectBlocksCount * _indirectBlockCapacity)
            {
                return result;
            }
            
            // collect outdated blocks from last valid indirect block
            var indirectBlocksCount = fmb.IndirectBlocksCount;
            var blockNumberAmongIndirectPlacedBlocks = startBlockNumberInFile - GlobalConstant.MaxFileDirectBlocksCount;
            var indirectBlockIndex = blockNumberAmongIndirectPlacedBlocks / _indirectBlockCapacity;
            var indirectBlock = await _volumeReader.ReadBlocks(fmb.IndirectBlocks[indirectBlockIndex]);
            result.AddRange(indirectBlock.ToIntArray().Skip(blockNumberAmongIndirectPlacedBlocks).Where(bn => bn != 0));
            
            // collect outdated indirect blocks and their outdated data blocks
            var indirectBlockNumbers = fmb.IndirectBlocks
                .Skip(indirectBlockIndex + 1)
                .Take(indirectBlocksCount - indirectBlockIndex)
                .Where(bn => bn != 0);
            foreach (var indirectBlockNumber in indirectBlockNumbers)
            {
                result.Add(indirectBlockNumber);
                result.AddRange((await _volumeReader.ReadBlocks(indirectBlockNumber)).ToIntArray().Where(bn => bn != 0));
                
                fmb.IndirectBlocksCount--;    // shrink indirect blocks
            }

            return result;
        }

        private ValueTask SaveFileMetaBlock(FileMetaBlock fileMetaBlock)
        {
            var fileMetaBlockData = _fileMetaBlockSerializer.Serialize(fileMetaBlock);
            return _volumeWriter.WriteBlock(fileMetaBlockData, fileMetaBlock.BlockNumber);
        }
    }
}