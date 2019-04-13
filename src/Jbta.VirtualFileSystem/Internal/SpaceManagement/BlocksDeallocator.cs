using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.Utils;

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
        
        public async Task DeallocateBlock(int blockNumber)
        {
            if (_bitmap.TryUnsetBit(blockNumber))
            {
                await _bitmap.SaveBitmapModifications(new []{blockNumber});
            }
        }

        public async Task DeallocateBlocks(IReadOnlyList<int> blocksNumbers)
        {
            var batchesCount = blocksNumbers.Count.DivideWithUpRounding(BatchSize);
            if (batchesCount == 0)
            {
                return;
            }
            
            foreach (var batchNumber in Enumerable.Range(0, batchesCount))
            {
                var batchOfBlocksNumbers = blocksNumbers.Skip(batchNumber * BatchSize).Take(BatchSize).ToArray();
                _bitmap.UnsetBits(batchOfBlocksNumbers);
            }
            
            await _bitmap.SaveBitmapModifications(blocksNumbers);
        }
    }
}