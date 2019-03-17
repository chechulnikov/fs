using System;
using System.IO;
using System.Threading.Tasks;

namespace Vfs
{
    internal class Volume : IDisposable
    {
        private readonly ushort _blockSize;
        private readonly FileStream _stream;

        public Volume(string volumePath, ushort blockSize)
        {
            _blockSize = blockSize;
            _stream = new FileStream(volumePath, FileMode.Open);
        }

        public async ValueTask<ReadOnlyMemory<byte>> ReadBlocks(int start, int blocksCount)
        {
            var buffer = new byte[blocksCount * _blockSize];
            await _stream.ReadAsync(buffer, start, blocksCount);
            return buffer;
        }

        public void FlushData(ReadOnlyMemory<byte> data)
        {
            var wholeBloks = data.Length / _blockSize;
            _stream.WriteAsync(data);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}