using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.Indexing;
using Jbta.VirtualFileSystem.Internal.SpaceManagement;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileCreator
    {
        private readonly FileSystemIndex _fileSystemIndex;
        private readonly IBinarySerializer<FileMetaBlock> _fileMetaBlockSerializer;
        private readonly BlocksAllocator _blocksAllocator;
        private readonly IVolumeWriter _volumeWriter;

        public FileCreator(
            FileSystemIndex fileSystemIndex,
            IBinarySerializer<FileMetaBlock> fileMetaBlockSerializer,
            BlocksAllocator blocksAllocator,
            IVolumeWriter volumeWriter)
        {
            _fileSystemIndex = fileSystemIndex;
            _fileMetaBlockSerializer = fileMetaBlockSerializer;
            _blocksAllocator = blocksAllocator;
            _volumeWriter = volumeWriter;
        }

        public async Task CreateFile(string fileName)
        {
            if (_fileSystemIndex.FileExists(fileName))
            {
                throw new FileSystemException($"File \"{fileName}\" has already existed");
            }

            var fileMetaBlockNumber = await _blocksAllocator.AllocateBlock();
            var fileMetaBlock = new FileMetaBlock { BlockNumber = fileMetaBlockNumber };
            var fileMetaBlockData = _fileMetaBlockSerializer.Serialize(fileMetaBlock);
            await _volumeWriter.WriteBlock(fileMetaBlockData, fileMetaBlockNumber);

            if (!await _fileSystemIndex.Insert(fileName, fileMetaBlockNumber))
            {
                throw new FileSystemException($"Can not insert file \"{fileName}\" to file system index");
            }
        } 
    }
}