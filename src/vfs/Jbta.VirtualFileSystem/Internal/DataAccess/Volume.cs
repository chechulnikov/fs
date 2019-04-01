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
            
            using (var stream = System.IO.File.OpenRead(VolumePath))
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
            using (var stream = System.IO.File.OpenRead(VolumePath))
            {
                stream.Seek(startBlockNumber * _blockSize, SeekOrigin.Begin);
                await stream.ReadAsync(buffer, startBlockNumber * _blockSize, blocksCount * _blockSize);
            }
            return buffer;
        }
        
        public async ValueTask WriteBlocksСontiguously(byte[] data, int startBlockNumber, int blocksCount)
        {
            if (startBlockNumber < 0) throw new ArgumentOutOfRangeException(nameof(startBlockNumber));
            if (blocksCount <= 0) throw new ArgumentOutOfRangeException(nameof(blocksCount));
            
            using (var stream = System.IO.File.OpenWrite(VolumePath))
            {
                stream.Seek(startBlockNumber * _blockSize, SeekOrigin.Begin);
                await stream.WriteAsync(data, startBlockNumber * _blockSize, blocksCount * _blockSize);
            }
        }
        
        public async ValueTask WriteBlocks(byte[] data, IReadOnlyList<int> blocksNumbers)
        {
            if (data.Length % _blockSize != 0)
                throw new FileSystemException("Invalid data size");

            using (var stream = System.IO.File.OpenWrite(VolumePath))
            {
                var startBlockNumberInChunk = blocksNumbers[0];
                var blocksCountInChunk = 1;

                for (var i = 1; i < blocksNumbers.Count; i++)
                {
                    if (blocksNumbers[i - 1] + 1 == blocksNumbers[i])
                    {
                        blocksCountInChunk++;
                        continue;
                    }

                    stream.Seek(startBlockNumberInChunk * _blockSize, SeekOrigin.Begin);
                    await stream.WriteAsync(data, startBlockNumberInChunk * _blockSize, blocksCountInChunk * _blockSize);

                    startBlockNumberInChunk = blocksNumbers[i];
                    blocksCountInChunk = 1;
                }
            }
        }
    }
}