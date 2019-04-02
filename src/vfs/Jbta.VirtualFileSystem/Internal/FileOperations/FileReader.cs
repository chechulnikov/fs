using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
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

        public async Task<Memory<byte>> Read(FileMetaBlock fileMetaBlock, int offsetInBytes, int lengthInBytes)
        {
            var startBlockNumberInFile = BytesToBlockNumber(offsetInBytes);
            var blocksCountForReading = BytesToBlockNumber(lengthInBytes);
            
            var directBlocksCountForReading = CalcDirectBlocksCountForReading(fileMetaBlock, startBlockNumberInFile, blocksCountForReading);
            var indirectBlocksCountForReading = blocksCountForReading - directBlocksCountForReading;

            var buffer = new byte[blocksCountForReading * _fileSystemMeta.BlockSize];
            var bufferOffset = 0;
            if (startBlockNumberInFile < GlobalConstant.MaxFileDirectBlocksCount)
            {
                // read direct blocks
                bufferOffset = await ReadByChunks(buffer, bufferOffset, fileMetaBlock.DirectBlocks, startBlockNumberInFile, directBlocksCountForReading);
            }
            if (indirectBlocksCountForReading > 0)
            {
                // read indirect blocks
                var startIndirectBlockIndexInFile = CalcStartIndirectBlockNumberInFile(startBlockNumberInFile);
                await ReadIndirectBlocks(buffer, bufferOffset, fileMetaBlock, startIndirectBlockIndexInFile, indirectBlocksCountForReading);
            }

            return new Memory<byte>(buffer, offsetInBytes - startBlockNumberInFile * _fileSystemMeta.BlockSize, lengthInBytes);
        }

        private int BytesToBlockNumber(int lengthInBytes) =>
            lengthInBytes.DivideWithUpRounding(_fileSystemMeta.BlockSize);

        private async Task ReadIndirectBlocks(
            byte[] buffer, int bufferOffset, FileMetaBlock fileMetaBlock, int startBlockNumberInFileForIndirectBlocks, int blocksCountForReading)
        {
            var dataBlocksCountPerIndirectBlock = _fileSystemMeta.BlockSize / sizeof(int);
            var startIndirectBlocksIndex = startBlockNumberInFileForIndirectBlocks.DivideWithUpRounding(dataBlocksCountPerIndirectBlock);
            var indirectBlocksCount = blocksCountForReading.DivideWithUpRounding(dataBlocksCountPerIndirectBlock);

            // read indirect blocks
            var indirectBlocksData = new byte[indirectBlocksCount * _fileSystemMeta.BlockSize];
            await ReadByChunks(indirectBlocksData, 0, fileMetaBlock.IndirectBlocks, startIndirectBlocksIndex, indirectBlocksCount);
            var dataBlocksNumbers = indirectBlocksData.ToIntArray();

            // read data blocks by numbers from indirect blocks
            await ReadByChunks(buffer, bufferOffset, dataBlocksNumbers, 0, dataBlocksNumbers.Length);
        }

        private static int CalcDirectBlocksCountForReading(
            FileMetaBlock fileMetaBlock, int startBlockNumberInFile, int blocksCountForReading)
        {
            var endBlockNumberInFile = startBlockNumberInFile + blocksCountForReading;
            return endBlockNumberInFile < fileMetaBlock.DirectBlocksCount
                ? endBlockNumberInFile
                : fileMetaBlock.DirectBlocksCount;
        }

        private static int CalcStartIndirectBlockNumberInFile(int startBlockNumberInFile)
        {
            return startBlockNumberInFile < GlobalConstant.MaxFileDirectBlocksCount
                ? 0
                : startBlockNumberInFile - GlobalConstant.MaxFileDirectBlocksCount;
        }

        private async Task<int> ReadByChunks(
            byte[] buffer, int bufferOffset, IReadOnlyList<int> blocksNumbers, int startBlockIndex, int blocksCount)
        {
            var startBlockNumberInChunk = blocksNumbers[startBlockIndex];
            var blocksCountInChunk = 1;
            for (var i = startBlockIndex; i < blocksCount; i++)
            {
                if (blocksNumbers[i - 1] + 1 == blocksNumbers[i])
                {
                    blocksCountInChunk++;
                    continue;
                }

                var length = blocksCountInChunk * _fileSystemMeta.BlockSize;
                var memory = new Memory<byte>(buffer, bufferOffset, length);
                
                await _volumeReader.ReadBlocksToBuffer(memory, startBlockNumberInChunk);

                startBlockNumberInChunk = blocksNumbers[i];
                blocksCountInChunk = 1;
                bufferOffset += length + 1;
            }

            return bufferOffset;
        }
    }
}