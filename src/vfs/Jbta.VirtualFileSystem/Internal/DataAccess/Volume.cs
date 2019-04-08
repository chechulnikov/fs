using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;

namespace Jbta.VirtualFileSystem.Internal.DataAccess
{
    internal class Volume : IVolumeReader, IVolumeWriter
    {
        private readonly int _blockSize;

        public Volume(string volumePath, int blockSize)
        {
            VolumePath = volumePath;
            _blockSize = blockSize;
        }
        
        public string VolumePath { get; }
        
        public async ValueTask<Memory<byte>> ReadBlocksToBuffer(Memory<byte> buffer, int startBlockNumber)
        {
            if (startBlockNumber < 0) throw new ArgumentOutOfRangeException(nameof(startBlockNumber));
            
            using (var stream = System.IO.File.Open(VolumePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.Seek(startBlockNumber * _blockSize, SeekOrigin.Begin);
                await stream.ReadAsync(buffer);
            }
            return buffer;
        }

        public async ValueTask<byte[]> ReadBlocks(int startBlockNumber, int blocksCount = 1)
        {
            if (startBlockNumber < 0) throw new ArgumentOutOfRangeException(nameof(startBlockNumber));
            if (blocksCount <= 0) throw new ArgumentOutOfRangeException(nameof(blocksCount));

            var buffer = new byte[blocksCount * _blockSize]; // TODO byte[] pool?
            using (var stream = System.IO.File.Open(VolumePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.Seek(startBlockNumber * _blockSize, SeekOrigin.Begin);
                await stream.ReadAsync(buffer);
            }
            return buffer;
        }

        public async ValueTask WriteBlock(byte[] data, int blockNumber)
        {
            if (data.Length % _blockSize != 0)
                throw new FileSystemException("Invalid data size");

            using (var stream = System.IO.File.Open(VolumePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.Seek(blockNumber * _blockSize, SeekOrigin.Begin);
                await stream.WriteAsync(data);
            }
        }

        public async ValueTask WriteBlocks(byte[] data, IReadOnlyList<int> blocksNumbers)
        {
            if (data.Length % _blockSize != 0)
                throw new FileSystemException("Invalid data size");

            using (var stream = System.IO.File.Open(VolumePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var startBlockNumberInChunk = blocksNumbers[0];
                var blocksCountInChunk = 1;
                var bufferOffset = 0;
                for (var i = 1; i < blocksNumbers.Count; i++)
                {
                    if (blocksNumbers[i - 1] + 1 == blocksNumbers[i] && i != blocksNumbers.Count - 1)
                    {
                        blocksCountInChunk++;
                        continue;
                    }

                    var bytesInChunk = blocksCountInChunk * _blockSize;
                    stream.Seek(startBlockNumberInChunk * _blockSize, SeekOrigin.Begin);
                    await stream.WriteAsync(data, bufferOffset, bytesInChunk);

                    startBlockNumberInChunk = blocksNumbers[i];
                    blocksCountInChunk = 1;
                    
                    bufferOffset += bytesInChunk;
                }
            }
        }
    }
}