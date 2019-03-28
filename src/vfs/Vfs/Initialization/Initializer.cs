using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vfs.Utils;

namespace Vfs.Initialization
{
    internal class Initializer
    {
        private const ushort MinAllocationGroupBlocksCount = 8197;
        
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
            
            if (settings.BlocksCountPerAllocationGroup < MinAllocationGroupBlocksCount)
                throw new FileSystemInitException(
                    $"Allocation group size cannot be less than {MinAllocationGroupBlocksCount} blocks");
        }

        private static Superblock CreateSuperblock(FileSystemSettings settings) => new Superblock
        {
            MagicNumber = GlobalConstant.SuperblockMagicNumber,
            BlockSize = settings.BlockSize,
            BlocksCount = settings.BlocksCountPerAllocationGroup + 1,
            UsedBlocksCount = 1, // TODO учитывать битмапы?
            InodeSize = 1,
            BlocksCountPerAllocationGroup = settings.BlocksCountPerAllocationGroup,
            AllocationGroupsCount = 1
        };

        private static ValueTask CreateVolume(FileSystemSettings settings, Superblock superblock)
        {
            var volume = new Volume(settings.VolumePath, settings.BlockSize);
            var superblockData = superblock.Serialize().ToArray();
            return volume.WriteBlocks(superblockData, Enumerable.Range(0, 1).ToArray());
        }
    }
}