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
        private readonly FileFactory _fileFactory;
        private readonly IBinarySerializer<FileMetaBlock> _fileMetaBlockSerializer;
        private readonly BlocksAllocator _blocksAllocator;
        private readonly IVolumeWriter _volumeWriter;

        public FileCreator(
            FileSystemIndex fileSystemIndex,
            FileFactory fileFactory,
            IBinarySerializer<FileMetaBlock> fileMetaBlockSerializer,
            BlocksAllocator blocksAllocator,
            IVolumeWriter volumeWriter)
        {
            _fileSystemIndex = fileSystemIndex;
            _fileFactory = fileFactory;
            _fileMetaBlockSerializer = fileMetaBlockSerializer;
            _blocksAllocator = blocksAllocator;
            _volumeWriter = volumeWriter;
        }

        public async Task<IFile> CreateFile(string fileName)
        {
            if (fileName.Length > GlobalConstant.MaxFileNameSize)
            {
                throw new FileSystemException($"File name cannot be greater then {GlobalConstant.MaxFileNameSize} symbols");
            }

            if (_fileSystemIndex.FileExists(fileName))
            {
                throw new FileSystemException($"File \"{fileName}\" has already existed");
            }
            
            var fileMetaBlock = new FileMetaBlock();
            
            var fileMetaBlockNumber = await _blocksAllocator.AllocateBlock();
            var fileMetaBlockData = _fileMetaBlockSerializer.Serialize(fileMetaBlock);
            await _volumeWriter.WriteBlock(fileMetaBlockData, fileMetaBlockNumber);

            if (!await _fileSystemIndex.Insert(fileName, fileMetaBlockNumber))
            {
                throw new FileSystemException($"Can not insert file \"{fileName}\" to file system index");
            }
            
            return _fileFactory.New(fileMetaBlock, fileName);
        } 
    }
}