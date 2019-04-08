using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.SpaceManagement;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileWriter
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly BlocksAllocator _blocksAllocator;
        private readonly IBinarySerializer<FileMetaBlock> _fileMetaBlockSeralizer;
        private readonly IVolumeReader _volumeReader;
        private readonly IVolumeWriter _volumeWriter;

        public FileWriter(
            FileSystemMeta fileSystemMeta,
            BlocksAllocator blocksAllocator,
            IBinarySerializer<FileMetaBlock> fileMetaBlockSeralizer,
            IVolumeReader volumeReader,
            IVolumeWriter volumeWriter)
        {
            _fileSystemMeta = fileSystemMeta;
            _blocksAllocator = blocksAllocator;
            _fileMetaBlockSeralizer = fileMetaBlockSeralizer;
            _volumeReader = volumeReader;
            _volumeWriter = volumeWriter;
        }

        public async Task Write(FileMetaBlock fileMetaBlock, int offsetInBytes, byte[] data)
        {
            var startBlockNumberInFile = BytesToBlockNumber(offsetInBytes);
            var blocksCountForWriting = BytesToBlockNumber(data.Length);
            
            var fileDataBlocksCount = CalcDataBlocksCount(fileMetaBlock, _fileSystemMeta.BlockSize);
            
            var blocksData = data;

            // 1. update existed blocks
            var updatingBlocksCount = fileDataBlocksCount - startBlockNumberInFile;
            if (updatingBlocksCount > 0)
            {
                // 1.0. update first block
                var startByteIndex = offsetInBytes - offsetInBytes / _fileSystemMeta.BlockSize * _fileSystemMeta.BlockSize;
                var dataForUpdate = data.Take(updatingBlocksCount * _fileSystemMeta.BlockSize - startByteIndex).ToArray();
                var firstBlockNumber = await GetBlockNumber(fileMetaBlock, startBlockNumberInFile);
                var firstBlock = await _volumeReader.ReadBlocks(firstBlockNumber, 1); // todo
                using (var stream = new MemoryStream(firstBlock))
                {
                    stream.Seek(startByteIndex, SeekOrigin.Begin);
                    stream.Write(dataForUpdate);
                }
                
                // 1.1. update direct blocks
                if (startBlockNumberInFile < GlobalConstant.MaxFileDirectBlocksCount)
                {
                    var directBlockNumbers = fileMetaBlock.DirectBlocks.Skip(startBlockNumberInFile + 1).Take(updatingBlocksCount - 1);
                    foreach (var directBlockNumber in directBlockNumbers)
                    {
                        // todo
                        //await _volume.WriteBlocks()
                    }
                    //todo
                }
                
                // 1.2. update indirect blocks
                // todo
            }
            
            // 2. add new blocks
            // todo shrink data from start
            var addingBlocksCount = blocksCountForWriting - updatingBlocksCount;
            if (addingBlocksCount > 0)
            {
                // 2.0. write data blocks
                Array.Resize(ref blocksData, _fileSystemMeta.BlockSize * addingBlocksCount);
                
                var reservedBlocksNumbers = await _blocksAllocator.AllocateBlocks(addingBlocksCount);
                await _volumeWriter.WriteBlocks(blocksData, reservedBlocksNumbers);
                
                // add to file meta block
                // 2.1. adding to direct blocks list
                var freeDirectBlocksCount = GlobalConstant.MaxFileDirectBlocksCount - fileMetaBlock.DirectBlocksCount;
                if (freeDirectBlocksCount > 0)
                {
                    foreach (var dataBlockNumber in reservedBlocksNumbers.Take(freeDirectBlocksCount))
                    {
                        fileMetaBlock.DirectBlocks[fileMetaBlock.DirectBlocksCount] = dataBlockNumber;
                        fileMetaBlock.DirectBlocksCount++;
                    }
                }
                
                // 2.2. adding to indirect blocks list
                var blocksNumbersForIndirectPlacement = reservedBlocksNumbers.Skip(freeDirectBlocksCount).ToArray();
                if (blocksNumbersForIndirectPlacement.Any())
                {
                    var reservedIndirectBlocksNumbers = await _blocksAllocator
                        .AllocateBytes(blocksNumbersForIndirectPlacement.Length / sizeof(int));

                    var newIndirectBlocks = blocksNumbersForIndirectPlacement.ToByteArray();
                    await _volumeWriter.WriteBlocks(newIndirectBlocks, reservedIndirectBlocksNumbers);
                    
                    foreach (var dataBlockNumber in reservedIndirectBlocksNumbers)
                    {
                        fileMetaBlock.IndirectBlocks[fileMetaBlock.IndirectBlocksCount] = dataBlockNumber;
                        fileMetaBlock.IndirectBlocksCount++;
                    }
                }
            }
            
            // update file meta block
            await SaveFileMetaBlock(fileMetaBlock);
        }

        private int BytesToBlockNumber(int bytesCount)
        {
            var div = bytesCount / _fileSystemMeta.BlockSize;
            return bytesCount % _fileSystemMeta.BlockSize == 0 ? div : div + 1;
        }

        private static int CalcDataBlocksCount(FileMetaBlock fileMetaBlock, int blockSize)
        {
            var indirectBlockCapacity = blockSize / sizeof(int);
            return fileMetaBlock.DirectBlocksCount + fileMetaBlock.IndirectBlocksCount * indirectBlockCapacity;
        }

        private async Task<int> GetBlockNumber(FileMetaBlock fileMetaBlock, int blockNumberInFile)
        {
            if (blockNumberInFile < GlobalConstant.MaxFileDirectBlocksCount)
            {
                return fileMetaBlock.DirectBlocks[blockNumberInFile];
            }

            var t = blockNumberInFile - GlobalConstant.MaxFileDirectBlocksCount; // todo rename
            var dataBlocksPerIndirectBlock = _fileSystemMeta.BlockSize / sizeof(int);
            var div = t / dataBlocksPerIndirectBlock;
            var remaining = t % dataBlocksPerIndirectBlock;
            var indirectBlockIndex = remaining == 0 ? div : div + 1;

            // todo without reading from volume: needs cache direct and indirect blocks
            var indirectBlock = await _volumeReader.ReadBlocks(fileMetaBlock.IndirectBlocks[indirectBlockIndex], 1);

            return BitConverter.ToInt32(indirectBlock, t - dataBlocksPerIndirectBlock * (indirectBlockIndex - 1));
        }

        private async Task SaveFileMetaBlock(FileMetaBlock fileMetaBlock)
        {
            var fileMetaBlockData = _fileMetaBlockSeralizer.Serialize(fileMetaBlock);
            await _volumeWriter.WriteBlock(fileMetaBlockData, fileMetaBlock.BlockNumber);
        }
    }
}