using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileSizeMeter
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly IVolumeReader _volumeReader;

        public FileSizeMeter(FileSystemMeta fileSystemMeta, IVolumeReader volumeReader)
        {
            _fileSystemMeta = fileSystemMeta;
            _volumeReader = volumeReader;
        }

        public async Task<int> MeasureSize(FileMetaBlock fileMetaBlock)
        {
            var dataBlocksCount =
                fileMetaBlock.DirectBlocksCount + await GetIndirectlyPlacedDataBlocksCount(fileMetaBlock);
            return (1 + dataBlocksCount) * _fileSystemMeta.BlockSize;
        }

        private async Task<int> GetIndirectlyPlacedDataBlocksCount(FileMetaBlock fileMetaBlock)
        {
            var result = 0;
            for (var i = 0; i < fileMetaBlock.IndirectBlocksCount; i++)
            {
                var indirectBlockData = await _volumeReader.ReadBlocks(fileMetaBlock.IndirectBlocks[i]);
                result += indirectBlockData.ToIntArray().Count(bn => bn != 0);
            }
            return result;
        }
    }
}