using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vfs
{
    internal class Allocator
    {
        private readonly Volume _volume;
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly BitmapTree _bitmap;

        public Allocator(Volume volume, IFileSystemMeta fileSystemMeta)
        {
            _volume = volume;
            _fileSystemMeta = fileSystemMeta;

            var bitmapBlocks = volume.ReadBlocks(1, fileSystemMeta.BlocksCount).Result;
            _bitmap = new BitmapTree(bitmapBlocks);
        }

        /// <summary>
        /// Allocates blocks for <param name="data">data byte array</param>
        /// </summary>
        /// <param name="data">Data byte array, th at should be allocated</param>
        /// <returns>Count of allocated bytes</returns>
        public async ValueTask<int> Allocate(byte[] data)
        {
            var blocksCount = CalcBlocksCount(data);
            var blocksNumbers = ReserveBlocks(blocksCount);

            await _volume.WriteBlocks(data, blocksNumbers);

            return blocksCount * _fileSystemMeta.BlockSize;
        }
        
        private int CalcBlocksCount(IReadOnlyCollection<byte> data)
        {
            var blocksCount = data.Count / _fileSystemMeta.BlockSize;
            return data.Count % _fileSystemMeta.BlockSize == 0 ? blocksCount : blocksCount + 1;
        }

        private IReadOnlyList<int> ReserveBlocks(int countOfBlocks)
        {
            var blocksNumbers = new int[countOfBlocks];

            blocksNumbers[0] = _bitmap.SetFirstUnsetBit();
            for (var i = 1; i < countOfBlocks; i++)
            {
                var blockNumber = blocksNumbers[i - 1] + 1;
                blocksNumbers[i] = _bitmap.TrySetBit(blockNumber) ? blockNumber : _bitmap.SetFirstUnsetBit();
            }

            return blocksNumbers;
        }
    }
}