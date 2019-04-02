using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.Initialization
{
    internal class Initializer
    {
        private readonly IBinarySerializer<Superblock> _superblockSerializer;
        private readonly IBinarySerializer<IndexBlock> _indexBlockSerializer;
        
        public Initializer(
            IBinarySerializer<Superblock> superblockSerializer,
            IBinarySerializer<IndexBlock> indexBlockSerializer)
        {
            _superblockSerializer = superblockSerializer;
            _indexBlockSerializer = indexBlockSerializer;
        }
        
        public ValueTask Initialize(string volumePath)
        {
            var superblock = CreateSuperblock();
            var bitmap = CreateBitmap();
            var rootIndexBlock = new IndexBlock();
            return CreateVolume(volumePath, superblock, bitmap, rootIndexBlock);
        }

        private static Superblock CreateSuperblock() => new Superblock
        {
            MagicNumber = GlobalConstant.SuperblockMagicNumber,
            BlockSize = GlobalConstant.BlockSize,
            IsDirty = false,
            RootIndexBlockNumber = GlobalConstant.BlockSize * GlobalConstant.BitmapBlocksCount + 1
        };

        private static BitArray CreateBitmap()
        {
            var bitArray = new BitArray(8 * GlobalConstant.BlockSize * GlobalConstant.BitmapBlocksCount);
            foreach (var i in Enumerable.Range(0, GlobalConstant.BitmapBlocksCount + 2))
            {
                bitArray.Set(i, true);
            }
            return bitArray;
        }

        private async ValueTask CreateVolume(
            string volumePath, Superblock superblock, BitArray bitmap, IndexBlock indexBlock)
        {
            var volume = new Volume(volumePath, GlobalConstant.BlockSize);
            await volume.WriteBlock(_superblockSerializer.Serialize(superblock), 0);
            await volume.WriteBlocks(bitmap.ToByteArray(), Enumerable.Range(1, GlobalConstant.BitmapBlocksCount).ToArray());
            await volume.WriteBlock(_indexBlockSerializer.Serialize(indexBlock), GlobalConstant.BitmapBlocksCount + 1);
        }
    }
}