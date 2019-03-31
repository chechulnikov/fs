using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Impl;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Initialization
{
    internal class Initializer
    {
        public ValueTask Initialize(FileSystemSettings settings)
        {
            ValidateSettings(settings);
            
            var superblock = CreateSuperblock(settings);
            return CreateVolume(settings, superblock);
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

        private static ValueTask CreateVolume(FileSystemSettings settings, Superblock superblock)
        {
            var volume = new Volume(settings.VolumePath, settings.BlockSize);
            var superblockData = superblock.Serialize().ToArray();
            return volume.WriteBlocks(superblockData, Enumerable.Range(0, 1).ToArray());
        }
    }
}