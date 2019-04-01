using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Impl.Indexing;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileCreator
    {
        private readonly FileSystemIndex _fileSystemIndex;
        private readonly FileFactory _fileFactory;
        private readonly Allocator _allocator;
        private readonly IVolumeWriter _volumeWriter;

        public FileCreator(
            FileSystemIndex fileSystemIndex,
            FileFactory fileFactory,
            Allocator allocator,
            IVolumeWriter volumeWriter)
        {
            _fileSystemIndex = fileSystemIndex;
            _fileFactory = fileFactory;
            _allocator = allocator;
            _volumeWriter = volumeWriter;
        }

        public async Task<IFile> CreateFile(string fileName)
        {
            if (fileName.Length > GlobalConstant.MaxFileNameSize)
            {
                throw new FileSystemException($"File name cannot be greater then {GlobalConstant.MaxFileNameSize} symbols");
            }

            if (_fileSystemIndex.Exists(fileName))
            {
                throw new FileSystemException($"File \"{fileName}\" has already existed");
            }
            
            var fileMetaBlock = new FileMetaBlock();
            var reservedBlocksNumbers = _allocator.AllocateBlocks(1);
            await _volumeWriter.WriteBlocks(fileMetaBlock.Serialize(), reservedBlocksNumbers);
            return _fileFactory.New(fileMetaBlock, fileName);
        } 
    }
}