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
        public ValueTask Initialize(FileSystemSettings settings)
        {
            ValidateSettings(settings);
            
            var superblock = CreateSuperblock(settings);
            var bitmap = CreateBitmap(settings);
            var rootIndexBlock = CreateRootIndexBlock(settings);
            return CreateVolume(settings, superblock, bitmap, rootIndexBlock);
        }

        private static void ValidateSettings(FileSystemSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.VolumePath) || System.IO.File.Exists(settings.VolumePath))
                throw new FileSystemInitException($"Volume \"{settings.VolumePath}\" cannot be created");

            switch (settings.BlockSize)
            {
                case 1024:
                case 2048:
                case 4096:
                case 8197:
                    break;
                default:
                    throw new FileSystemInitException(
                        $"Invalid block size {settings.BlockSize}. 1024, 2048, 4096, 8197 are only allowed");
            }
        }

        private static Superblock CreateSuperblock(FileSystemSettings settings) => new Superblock
        {
            MagicNumber = GlobalConstant.SuperblockMagicNumber,
            BlockSize = settings.BlockSize,
            BlocksCount = settings.BlockSize * sizeof(byte) * GlobalConstant.BitmapBlocksCount,
            UsedBlocksCount = 1 + GlobalConstant.BitmapBlocksCount
        };

        private static BitArray CreateBitmap(FileSystemSettings settings)
        {
            var bitArray = new BitArray(8 * settings.BlockSize * GlobalConstant.BitmapBlocksCount);
            foreach (var i in Enumerable.Range(0, GlobalConstant.BitmapBlocksCount))
            {
                bitArray.Set(i, true);
            }
            return bitArray;
        }

        private static IndexBlock CreateRootIndexBlock(FileSystemSettings settings) => new IndexBlock(settings.BlockSize);

        private static async ValueTask CreateVolume(
            FileSystemSettings settings, Superblock superblock, BitArray bitmap, IBinarySerializable indexBlock)
        {
            var volume = new Volume(settings.VolumePath, settings.BlockSize);
            await volume.WriteBlocks(superblock.Serialize().ToArray(), Enumerable.Range(0, 1).ToArray());
            await volume.WriteBlocks(bitmap.ToByteArray(), Enumerable.Range(1, GlobalConstant.BitmapBlocksCount).ToArray());
            await volume.WriteBlocks(indexBlock.Serialize(), Enumerable.Range(GlobalConstant.BitmapBlocksCount,1).ToArray());
        }
    }
}