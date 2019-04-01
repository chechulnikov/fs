using System.Collections.Generic;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class BlocksAllocator
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly BitmapTree _bitmapTree;

        public BlocksAllocator(FileSystemMeta fileSystemMeta, BitmapTree bitmapTree)
        {
            _fileSystemMeta = fileSystemMeta;
            _bitmapTree = bitmapTree;
        }

        /// <summary>
        /// Allocates bytes in blocks by given bytes count
        /// </summary>
        /// <param name="bytesCount">Count of bytes, that should be allocated</param>
        /// <returns>Allocated blocks numbers</returns>
        public IReadOnlyList<int> AllocateBytes(int bytesCount)
        {
            var blocksCount = CalcBlocksCount(bytesCount);
            return AllocateBlocks(blocksCount);
        }
        
        /// <summary>
        /// Allocates blocks of bytes by given blocks count
        /// </summary>
        /// <param name="blocksCount">Count of blocks of bytes, that should be allocated</param>
        /// <returns>Allocation result with count of allocated bytes and allocated blocks numbers</returns>
        public IReadOnlyList<int> AllocateBlocks(int blocksCount)
        {
            var blocksNumbers = new int[blocksCount];

            blocksNumbers[0] = _bitmapTree.SetFirstUnsetBit();
            for (var i = 1; i < blocksCount; i++)
            {
                var blockNumber = blocksNumbers[i - 1] + 1;
                blocksNumbers[i] = _bitmapTree.TrySetBit(blockNumber) ? blockNumber : _bitmapTree.SetFirstUnsetBit();
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