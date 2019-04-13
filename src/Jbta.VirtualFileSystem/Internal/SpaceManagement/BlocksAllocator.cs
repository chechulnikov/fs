using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Internal.SpaceManagement
{
    /// <summary>
    /// Finds free blocks
    /// </summary>
    internal class BlocksAllocator
    {
        private readonly Bitmap _bitmap;

        public BlocksAllocator(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        /// <summary>
        /// Allocates one blocks of bytes
        /// </summary>
        public async ValueTask<int> AllocateBlock()
        {
            var blockNumber = _bitmap.SetFirstUnsetBit();
            await _bitmap.SaveBitmapModifications(new []{blockNumber});
            return blockNumber;
        }

        /// <summary>
        /// Allocates blocks of bytes by given blocks count
        /// </summary>
        /// <param name="blocksCount">Count of blocks of bytes, that should be allocated</param>
        /// <returns>Allocation result with count of allocated bytes and allocated blocks numbers</returns>
        public async ValueTask<IReadOnlyList<int>> AllocateBlocks(int blocksCount)
        {
            var blocksNumbers = new int[blocksCount];

            blocksNumbers[0] = _bitmap.SetFirstUnsetBit();
            for (var i = 1; i < blocksCount; i++)
            {
                var blockNumber = blocksNumbers[i - 1] + 1;
                blocksNumbers[i] = _bitmap.TrySetBit(blockNumber) ? blockNumber : _bitmap.SetFirstUnsetBit();
            }

            await _bitmap.SaveBitmapModifications(blocksNumbers);

            return blocksNumbers;
        }
    }
}