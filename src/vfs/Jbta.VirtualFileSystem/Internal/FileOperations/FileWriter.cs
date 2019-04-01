using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.Blocks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.SpaceManagement;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileWriter
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly BlocksAllocator _blocksAllocator;
        private readonly IVolumeReader _volumeReader;
        private readonly IVolumeWriter _volumeWriter;

        public FileWriter(
            FileSystemMeta fileSystemMeta,
            BlocksAllocator blocksAllocator,
            IVolumeReader volumeReader,
            IVolumeWriter volumeWriter)
        {
            _fileSystemMeta = fileSystemMeta;
            _blocksAllocator = blocksAllocator;
            _volumeReader = volumeReader;
            _volumeWriter = volumeWriter;
        }

        public async Task Write(FileMetaBlock fileMetaBlock, int offsetInBytes, byte[] data)
        {
            var startBlockNumberInFile = BytesToBlockNumber(offsetInBytes);
            var blocksCountForWriting = BytesToBlockNumber(data.Length);
            
            var fileDataBlocksCount = CalcDataBlocksCount(fileMetaBlock, _fileSystemMeta.BlockSize);

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
                var reservedBlocksNumbers = await _blocksAllocator.AllocateBlocks(addingBlocksCount);
                await _volumeWriter.WriteBlocks(data, reservedBlocksNumbers);
                
                // add to file meta block
                // 2.1. adding to direct blocks list
                var freeDirectBlocksCount = GlobalConstant.MaxFileDirectBlocksCount - fileMetaBlock.DirectBlocksCount;
                if (freeDirectBlocksCount > 0)
                {
                    var i = fileMetaBlock.DirectBlocksCount + 1;
                    foreach (var dataBlockNumber in reservedBlocksNumbers.Take(freeDirectBlocksCount))
                    {
                        fileMetaBlock.DirectBlocks[i] = dataBlockNumber;
                        i++;
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
                    
                    var i = fileMetaBlock.DirectBlocksCount + 1;
                    foreach (var dataBlockNumber in reservedIndirectBlocksNumbers)
                    {
                        fileMetaBlock.IndirectBlocks[i] = dataBlockNumber;
                        i++;
                    }
                }
            }
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
    }
}