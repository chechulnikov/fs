using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.Indexing;
using Jbta.VirtualFileSystem.Internal.SpaceManagement;
using Jbta.VirtualFileSystem.Internal.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileRemover
    {
        private readonly FileSystemIndex _fileSystemIndex;
        private readonly BlocksDeallocator _blocksDeallocator;
        private readonly IBinarySerializer<FileMetaBlock> _fileMetaBlockDeserializer;
        private readonly IVolumeReader _volumeReader;

        public FileRemover(
            FileSystemIndex fileSystemIndex,
            BlocksDeallocator blocksDeallocator,
            IBinarySerializer<FileMetaBlock> fileMetaBlockDeserializer,
            IVolumeReader volumeReader)
        {
            _fileSystemIndex = fileSystemIndex;
            _blocksDeallocator = blocksDeallocator;
            _fileMetaBlockDeserializer = fileMetaBlockDeserializer;
            _volumeReader = volumeReader;
        }

        public async Task Remove(string fileName)
        {
            var (fileMetaBlockNumber, hasBeenFound) = _fileSystemIndex.SearchFile(fileName);
            if (!hasBeenFound)
            {
                throw new FileSystemException($"File \"{fileName}\" not found");
            }
            
            var fileBlocksNumbers = await LoadAllBLocksNumbers(fileMetaBlockNumber);
            await _blocksDeallocator.DeallocateBlocks(fileBlocksNumbers);

            if (!await _fileSystemIndex.RemoveFile(fileName))
            {
                throw new FileSystemException($"Can not remove file \"{fileName}\" from file system index");
            }
        }

        private async Task<FileMetaBlock> LoadFileMetaBlock(int fileMetaBlockNumber)
        {
            var fileMetaBlockData = await _volumeReader.ReadBlocks(fileMetaBlockNumber);
            var fileMetaBlock = _fileMetaBlockDeserializer.Deserialize(fileMetaBlockData);
            fileMetaBlock.BlockNumber = fileMetaBlockNumber;
            return fileMetaBlock;
        }

        private async Task<IReadOnlyList<int>> LoadAllBLocksNumbers(int fileMetaBlockNumber)
        {
            var fileMetaBlock = await LoadFileMetaBlock(fileMetaBlockNumber);
            var directBlocksNumber = fileMetaBlock.DirectBlocks.Take(fileMetaBlock.DirectBlocksCount);
            var indirectBlocksNumber = fileMetaBlock.IndirectBlocks.Take(fileMetaBlock.IndirectBlocksCount).ToArray();
            
            var result = new List<int> { fileMetaBlockNumber };
            result.AddRange(directBlocksNumber);
            result.AddRange(indirectBlocksNumber);

            foreach (var indirectBlockNumber in indirectBlocksNumber)
            {
                var indirectBlockData = await _volumeReader.ReadBlocks(indirectBlockNumber);
                result.AddRange(indirectBlockData.ToIntArray());
            }
            
            return result;
        }
    }
}