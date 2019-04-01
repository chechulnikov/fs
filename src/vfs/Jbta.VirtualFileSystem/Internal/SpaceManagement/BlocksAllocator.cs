using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Internal.SpaceManagement
{
    internal class BlocksAllocator
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly Bitmap _bitmap;

        public BlocksAllocator(FileSystemMeta fileSystemMeta, Bitmap bitmap)
        {
            _fileSystemMeta = fileSystemMeta;
            _bitmap = bitmap;
        }

        /// <summary>
        /// Allocates bytes in blocks by given bytes count
        /// </summary>
        /// <param name="bytesCount">Count of bytes, that should be allocated</param>
        /// <returns>Allocated blocks numbers</returns>
        public ValueTask<IReadOnlyList<int>> AllocateBytes(int bytesCount)
        {
            var blocksCount = CalcBlocksCount(bytesCount);
            return AllocateBlocks(blocksCount);
        }
        
        /// <summary>
        /// Allocates blocks of bytes by given blocks count
        /// </summary>
        /// <param name="blocksCount">Count of blocks of bytes, that should be allocated</param>
        /// <returns>Allocation result with count of allocated bytes and allocated blocks numbers</returns>
        public async ValueTask<IReadOnlyList<int>> AllocateBlocks(int blocksCount)
        {
            var blocksNumbers = new int[blocksCount];

            blocksNumbers[0] = await _bitmap.SetFirstUnsetBit();
            for (var i = 1; i < blocksCount; i++)
            {
                var blockNumber = blocksNumbers[i - 1] + 1;
                blocksNumbers[i] = await _bitmap.TrySetBit(blockNumber) ? blockNumber : await _bitmap.SetFirstUnsetBit();
            }

            return blocksNumbers;
        }

        private int CalcBlocksCount(int bytesCount)
        {
            var blocksCount = bytesCount / _fileSystemMeta.BlockSize;
            return bytesCount % _fileSystemMeta.BlockSize == 0 ? blocksCount : blocksCount + 1;
        }
    }
}