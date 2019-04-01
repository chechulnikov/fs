using System;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileReader
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly IVolumeReader _volumeReader;

        public FileReader(
            FileSystemMeta fileSystemMeta,
            IVolumeReader volumeReader)
        {
            _fileSystemMeta = fileSystemMeta;
            _volumeReader = volumeReader;
        }

        public async Task<byte[]> Read(FileMetaBlock fileMetaBlock, int offsetInBytes, int lengthInBytes)
        {
            var fileBlocksCount = fileMetaBlock.CalcDataBlocksCount(_fileSystemMeta.BlockSize);
            var startBlockNumberInFile = BytesToBlockNumber(offsetInBytes);
            var blocksCountForReading = BytesToBlockNumber(lengthInBytes);

            if (fileBlocksCount < blocksCountForReading)
            {
                throw new FileSystemException("Requested length is invalid");
            }
            
            var buffer = new byte[lengthInBytes];

            if (startBlockNumberInFile < GlobalConstant.FileDirectBlocksCount)
            {
                await ReadDirectBlocks(buffer, fileMetaBlock, startBlockNumberInFile, blocksCountForReading);
            }
            if (blocksCountForReading >= GlobalConstant.FileDirectBlocksCount + startBlockNumberInFile)
            {
                await ReadIndirectBlocks(buffer, fileMetaBlock);
            }

            return buffer;
        }

        private int BytesToBlockNumber(int lengthInBytes)
        {
            var div = lengthInBytes / _fileSystemMeta.BlockSize;
            return lengthInBytes % _fileSystemMeta.BlockSize == 0 ? div : div + 1;
        }
        
        private async Task ReadDirectBlocks(
            byte[] buffer, FileMetaBlock fileMetaBlock, int startBlockNumberInFile, int blocksCountForReading)
        {
            var endBlockNumberInFile = startBlockNumberInFile + blocksCountForReading;
            var maxBlockNumber = endBlockNumberInFile < fileMetaBlock.DirectBlocks.Count
                ? endBlockNumberInFile
                : fileMetaBlock.DirectBlocks.Count;
            
            var startBlockNumberInChunk = fileMetaBlock.DirectBlocks[startBlockNumberInFile];
            var blocksCountInChunk = 1;
            for (var i = 1; i < maxBlockNumber; i++)
            {
                if (fileMetaBlock.DirectBlocks[i - 1] + 1 == fileMetaBlock.DirectBlocks[i])
                {
                    blocksCountInChunk++;
                    continue;
                }

                var memory = new Memory<byte>(buffer, startBlockNumberInChunk,blocksCountInChunk * _fileSystemMeta.BlockSize);
                await _volumeReader.ReadBlocksToBuffer(memory, startBlockNumberInChunk);

                startBlockNumberInChunk = fileMetaBlock.DirectBlocks[i];
                blocksCountInChunk = 1;
            }
        }
        
        private Task ReadIndirectBlocks(byte[] buffer, FileMetaBlock fileMetaBlock)
        {
            // todo not implemented
            return Task.FromResult(0);
        }
    }
}