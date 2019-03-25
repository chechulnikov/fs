using System;
using System.IO;
using System.Threading.Tasks;

namespace Vfs
{
    internal class Volume : IDisposable
    {
        private readonly int _blockSize;
        private readonly FileStream _stream;

        public Volume(string volumePath, int blockSize)
        {
            _blockSize = blockSize;
            
            var fileMode = File.Exists(volumePath) ? FileMode.Open : FileMode.CreateNew;
            _stream = new FileStream(volumePath, fileMode);
        }

        public async ValueTask<ReadOnlyMemory<byte>> ReadBlocks(int start, int blocksCount)
        {
            var buffer = new byte[blocksCount * _blockSize]; // TODO byte[] pool?
            await _stream.ReadAsync(buffer, start, blocksCount);
            return buffer;
        }

        public async ValueTask WriteBlocks(byte[] data, int offset)
        {
            if (data.Length % _blockSize != 0)
            {
                throw new FileSystemException("Invalid data size");
            }
            
            var blocksCount = data.Length / _blockSize;
            await _stream.WriteAsync(data, offset, blocksCount);
        }

        public void Dispose() => _stream?.Dispose();
    }
}