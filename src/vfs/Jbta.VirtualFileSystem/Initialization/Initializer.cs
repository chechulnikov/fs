using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Impl;
using Jbta.VirtualFileSystem.Impl.Indexing;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Initialization
{
    internal class Initializer
    {
        public ValueTask Initialize(string volumePath)
        {
            var superblock = CreateSuperblock();
            var bitmap = CreateBitmap();
            var rootIndexBlock = CreateRootIndexBlock();
            return CreateVolume(volumePath, superblock, bitmap, rootIndexBlock);
        }

        private static Superblock CreateSuperblock() => new Superblock
        {
            MagicNumber = GlobalConstant.SuperblockMagicNumber,
            BlockSize = DefaultSettings.BlockSize,
            IsDirty = false
        };

        private static BitArray CreateBitmap()
        {
            var bitArray = new BitArray(8 * DefaultSettings.BlockSize * GlobalConstant.BitmapBlocksCount);
            foreach (var i in Enumerable.Range(0, GlobalConstant.BitmapBlocksCount))
            {
                bitArray.Set(i, true);
            }
            return bitArray;
        }

        private static IndexBlock CreateRootIndexBlock() => new IndexBlock(DefaultSettings.BlockSize);

        private static async ValueTask CreateVolume(
            string volumePath, Superblock superblock, BitArray bitmap, IBinarySerializable indexBlock)
        {
            var volume = new Volume(volumePath, DefaultSettings.BlockSize);
            await volume.WriteBlocks(superblock.Serialize().ToArray(), Enumerable.Range(0, 1).ToArray());
            await volume.WriteBlocks(bitmap.ToByteArray(), Enumerable.Range(1, GlobalConstant.BitmapBlocksCount).ToArray());
            await volume.WriteBlocks(indexBlock.Serialize(), Enumerable.Range(GlobalConstant.BitmapBlocksCount,1).ToArray());
        }
    }
}