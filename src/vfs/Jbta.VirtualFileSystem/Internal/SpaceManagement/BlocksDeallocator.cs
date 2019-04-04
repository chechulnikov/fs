using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.SpaceManagement
{
    /// <summary>
    /// Frees up using blocks
    /// </summary>
    internal class BlocksDeallocator
    {
        private const int BatchSize = 4096;
        private readonly Bitmap _bitmap;

        public BlocksDeallocator(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }
        
        public Task DeallocateBlock(int blockNumber) => _bitmap.TryUnsetBit(blockNumber);

        public async Task DeallocateBlocks(IReadOnlyList<int> blocksNumbers)
        {
            var batchesCount = blocksNumbers.Count.DivideWithUpRounding(BatchSize);
            foreach (var batchNumber in Enumerable.Range(0, batchesCount))
            {
                var batchOfBlocksNumbers = blocksNumbers.Skip(batchNumber * BatchSize).Take(BatchSize).ToArray();
                await _bitmap.UnsetBits(batchOfBlocksNumbers);
            }
        }
    }
}