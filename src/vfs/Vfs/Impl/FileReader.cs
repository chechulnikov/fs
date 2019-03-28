using System;
using System.Threading.Tasks;

namespace Vfs
{
    internal class FileReader
    {
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly TransactionManager _transactionManager;
        private readonly Volume _volume;

        public FileReader(
            IFileSystemMeta fileSystemMeta,
            TransactionManager transactionManager,
            Volume volume)
        {
            _fileSystemMeta = fileSystemMeta;
            _transactionManager = transactionManager;
            _volume = volume;
        }

        public async Task<byte[]> Read(FileMetaBlock fileMetaBlock, int offsetInBytes, int lengthInBytes)
        {
            using (var transaction = _transactionManager.StartTransaction())
            {
                var fileBlocksCount = CalcBlocksCountInFile(fileMetaBlock);
                var startBlockNumberInFile = BytesToBlockNumber(offsetInBytes);
                var blocksCountForReading = BytesToBlockNumber(lengthInBytes);

                if (fileBlocksCount < blocksCountForReading)
                {
                    throw new FileSystemException("Requested length is invalid");
                }
                
                var buffer = new byte[lengthInBytes];

                if (startBlockNumberInFile < GlobalConstant.CountOfDirectBlocks)
                {
                    await ReadDirectBlocks(buffer, fileMetaBlock, startBlockNumberInFile, blocksCountForReading);
                }
                if (blocksCountForReading >= GlobalConstant.CountOfDirectBlocks + startBlockNumberInFile)
                {
                    await ReadIndirectBlocks(buffer, fileMetaBlock);
                }

                return buffer;
            }
        }

        private int BytesToBlockNumber(int lengthInBytes)
        {
            var div = lengthInBytes / _fileSystemMeta.BlockSize;
            return lengthInBytes % _fileSystemMeta.BlockSize == 0 ? div : div + 1;
        }

        private int CalcBlocksCountInFile(FileMetaBlock fileMetaBlock)
        {
            var directBlocksCount = fileMetaBlock.DirectBlocks.Length;
            var indirectBlocksCount = fileMetaBlock.IndirectBlocks.Length;
            var indirectBlockCapacity = _fileSystemMeta.BlockSize / sizeof(int);
            return directBlocksCount + indirectBlocksCount * indirectBlockCapacity;
        }
        
        private async Task ReadDirectBlocks(
            byte[] buffer, FileMetaBlock fileMetaBlock, int startBlockNumberInFile, int blocksCountForReading)
        {
            var endBlockNumberInFile = startBlockNumberInFile + blocksCountForReading;
            var maxBlockNumber = endBlockNumberInFile < fileMetaBlock.DirectBlocks.Length
                ? endBlockNumberInFile
                : fileMetaBlock.DirectBlocks.Length;
            
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
                await _volume.ReadBlocksToBuffer(memory, startBlockNumberInChunk);

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