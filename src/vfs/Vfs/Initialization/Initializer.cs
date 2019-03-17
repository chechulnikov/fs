using System.IO;
using System.Threading.Tasks;
using Vfs.Utils;

namespace Vfs.Initialization
{
    internal class Initializer
    {
        private const ushort MinAllocationGroupBlocksCount = 8197;
        
        public Task Initialize(FileSystemSettings settings)
        {
            ValidateSettings(settings);
            return CreateVolume(settings);
        }

        private static void ValidateSettings(FileSystemSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.VolumePath) || File.Exists(settings.VolumePath))
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

        private static Task CreateVolume(FileSystemSettings settings)
        {
            var superblock = new Superblock
            {
                BlockSize = settings.BlockSize,
                BlocksCount = (uint) settings.BlocksCountPerAllocationGroup + 1,
                UsedBlocksCount = 1, // TODO учитывать битмапы?
                INodeSize = 1,
                BlocksCountPerAllocationGroup = settings.BlocksCountPerAllocationGroup,
                AllocationGroupsCount = 1
            };
            var volumeData = superblock.Serialize().ToArray();
            return File.WriteAllBytesAsync(settings.VolumePath, volumeData);
        }
    }
}