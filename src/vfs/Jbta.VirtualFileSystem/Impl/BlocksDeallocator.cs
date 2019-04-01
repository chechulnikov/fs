using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class BlocksDeallocator
    {
        private const int BatchSize = 4096;
        private readonly BitmapTree _bitmapTree;
        private readonly IVolumeWriter _volumeWriter;

        public BlocksDeallocator(
            BitmapTree bitmapTree,
            IVolumeWriter volumeWriter)
        {
            _bitmapTree = bitmapTree;
            _volumeWriter = volumeWriter;
        }

        public async Task DeallocateBlocks(IReadOnlyList<int> blocksNumbers)
        {
            var batchesCount = blocksNumbers.Count.DivideWithUpRounding(BatchSize);
            foreach (var batchNumber in Enumerable.Range(0, batchesCount))
            {
                var batchOfBlocksNumbers = blocksNumbers.Skip(batchNumber * BatchSize).Take(BatchSize);
                _bitmapTree.UnsetBits(batchOfBlocksNumbers);

                await SaveModifiedBitmapBlocks(blocksNumbers);
            }
        }
        
        private ValueTask SaveModifiedBitmapBlocks(IReadOnlyList<int> bitNumbers)
        {
            var modifiedBitmapBlocks = _bitmapTree.GetBitmapBlocks(bitNumbers);
            return _volumeWriter.WriteBlocks(modifiedBitmapBlocks, bitNumbers);
        }
    }
}