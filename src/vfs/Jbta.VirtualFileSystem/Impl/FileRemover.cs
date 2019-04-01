using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Impl.Blocks;
using Jbta.VirtualFileSystem.Impl.Indexing;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Impl
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
            var (fileMetaBlockNumber, hasBeenFound) = _fileSystemIndex.Search(fileName);
            if (!hasBeenFound)
            {
                throw new FileSystemException($"File \"{fileName}\" not found");
            }
            
            var fileBlocksNumbers = await LoadAllBLocksNumbers(fileMetaBlockNumber);
            await _blocksDeallocator.DeallocateBlocks(fileBlocksNumbers);
        }

        private async Task<FileMetaBlock> LoadFileMetaBlock(int fileMetaBlockNumber)
        {
            var fileMetaBlockData = await _volumeReader.ReadBlocks(fileMetaBlockNumber);
            return _fileMetaBlockDeserializer.Deserialize(fileMetaBlockData);
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