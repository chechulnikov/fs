using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileWriter
    {
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly Allocator _allocator;
        private readonly Volume _volume;

        public FileWriter(
            IFileSystemMeta fileSystemMeta,
            Allocator allocator,
            Volume volume)
        {
            _fileSystemMeta = fileSystemMeta;
            _allocator = allocator;
            _volume = volume;
        }

        public async Task Write(FileMetaBlock fileMetaBlock, int offsetInBytes, byte[] data)
        {
            var startBlockNumberInFile = BytesToBlockNumber(offsetInBytes);
            var blocksCountForWriting = BytesToBlockNumber(data.Length);
            
            var fileDataBlocksCount = fileMetaBlock.CalcDataBlocksCount(_fileSystemMeta.BlockSize);
            var fileDataBlocksSizeInBytes = fileMetaBlock.CalcDataBlocksSizeInBytes(_fileSystemMeta.BlockSize);

            // 1. update existed blocks
            var updatingBlocksCount = fileDataBlocksCount - startBlockNumberInFile;
            if (updatingBlocksCount > 0)
            {
                // 1.0. update first block
                var startByteIndex = offsetInBytes - offsetInBytes / _fileSystemMeta.BlockSize * _fileSystemMeta.BlockSize;
                var dataForUpdate = data.Take(updatingBlocksCount * _fileSystemMeta.BlockSize - startByteIndex).ToArray();
                var firstBlockNumber = await GetBlockNumber(fileMetaBlock, startBlockNumberInFile);
                var firstBlock = await _volume.ReadBlocks(firstBlockNumber, 1); // todo
                using (var stream = new MemoryStream(firstBlock))
                {
                    stream.Seek(startByteIndex, SeekOrigin.Begin);
                    stream.Write(dataForUpdate);
                }
                
                // 1.1. update direct blocks
                if (startBlockNumberInFile < GlobalConstant.FileDirectBlocksCount)
                {
                    var directBlockNumbers = fileMetaBlock.DirectBlocks.Skip(startBlockNumberInFile + 1).Take(updatingBlocksCount - 1);
                    foreach (var directBlockNumber in directBlockNumbers)
                    {
                        await _volume.WriteBlocks()
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
                var dataAllocationResult = _allocator.AllocateBlocks(addingBlocksCount);
                await _volume.WriteBlocks(data, dataAllocationResult.ReservedBlocks);
                
                // add to file meta block
                // 2.1. adding to direct blocks list
                var freeDirectBlocksCount = GlobalConstant.FileDirectBlocksCount - fileMetaBlock.DirectBlocks.Count;
                if (freeDirectBlocksCount > 0)
                {
                    var blockNumbersForDirectPlacement = dataAllocationResult.ReservedBlocks.Take(freeDirectBlocksCount);
                    foreach (var dataBlockNumber in blockNumbersForDirectPlacement)
                    {
                        fileMetaBlock.DirectBlocks.Add(dataBlockNumber);
                    }
                }
                
                // 2.2. adding to indirect blocks list
                var blocksNumbersForIndirectPlacement = dataAllocationResult.ReservedBlocks.Skip(freeDirectBlocksCount).ToArray();
                if (blocksNumbersForIndirectPlacement.Any())
                {
                    var indirectBlocksAllocationResult = _allocator
                        .AllocateBytes(blocksNumbersForIndirectPlacement.Length / sizeof(int));

                    var newIndirectBlocks = blocksNumbersForIndirectPlacement.ToByteArray();
                    await _volume.WriteBlocks(newIndirectBlocks, indirectBlocksAllocationResult.ReservedBlocks);
                    
                    foreach (var dataBlockNumber in indirectBlocksAllocationResult.ReservedBlocks)
                    {
                        fileMetaBlock.IndirectBlocks.Add(dataBlockNumber);
                    }
                }
            }
        }
        
        private int BytesToBlockNumber(int bytesCount)
        {
            var div = bytesCount / _fileSystemMeta.BlockSize;
            return bytesCount % _fileSystemMeta.BlockSize == 0 ? div : div + 1;
        }

        private async Task<int> GetBlockNumber(FileMetaBlock fileMetaBlock, int blockNumberInFile)
        {
            if (blockNumberInFile < GlobalConstant.FileDirectBlocksCount)
            {
                return fileMetaBlock.DirectBlocks[blockNumberInFile];
            }

            var t = blockNumberInFile - GlobalConstant.FileDirectBlocksCount; // todo rename
            var dataBlocksPerIndirectBlock = _fileSystemMeta.BlockSize / sizeof(int);
            var div = t / dataBlocksPerIndirectBlock;
            var remaining = t % dataBlocksPerIndirectBlock;
            var indirectBlockIndex = remaining == 0 ? div : div + 1;

            // todo without reading from volume: needs cache direct and indirect blocks
            var indirectBlock = await _volume.ReadBlocks(fileMetaBlock.IndirectBlocks[indirectBlockIndex], 1);

            return BitConverter.ToInt32(indirectBlock, t - dataBlocksPerIndirectBlock * (indirectBlockIndex - 1));
        }
    }
}