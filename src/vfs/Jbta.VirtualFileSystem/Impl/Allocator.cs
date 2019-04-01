using System.Collections.Generic;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class Allocator
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly BitmapTree _bitmap;

        public Allocator(FileSystemMeta fileSystemMeta, BitmapTree bitmap)
        {
            _fileSystemMeta = fileSystemMeta;
            _bitmap = bitmap;
        }

        /// <summary>
        /// Allocates bytes in blocks by given bytes count
        /// </summary>
        /// <param name="bytesCount">Count of bytes, that should be allocated</param>
        /// <returns>Allocation result with count of allocated bytes and allocated blocks numbers</returns>
        public AllocationResult AllocateBytes(int bytesCount)
        {
            var blocksCount = CalcBlocksCount(bytesCount);

            return new AllocationResult
            {
                BytesAllocated = blocksCount * _fileSystemMeta.BlockSize,
                ReservedBlocks = ReserveBlocks(blocksCount)
            };
        }
        
        /// <summary>
        /// Allocates blocks of bytes by given blocks count
        /// </summary>
        /// <param name="blocksCount">Count of blocks of bytes, that should be allocated</param>
        /// <returns>Allocation result with count of allocated bytes and allocated blocks numbers</returns>
        public AllocationResult AllocateBlocks(int blocksCount)
        {
            return new AllocationResult
            {
                BytesAllocated = blocksCount * _fileSystemMeta.BlockSize,
                ReservedBlocks = ReserveBlocks(blocksCount)
            };
        }
        
        private int CalcBlocksCount(int bytesCount)
        {
            var blocksCount = bytesCount / _fileSystemMeta.BlockSize;
            return bytesCount % _fileSystemMeta.BlockSize == 0 ? blocksCount : blocksCount + 1;
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