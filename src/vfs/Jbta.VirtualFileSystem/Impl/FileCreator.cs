using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileCreator
    {
        private readonly FileFactory _fileFactory;
        private readonly Allocator _allocator;
        private readonly IVolumeWriter _volumeWriter;

        public FileCreator(
            FileFactory fileFactory,
            Allocator allocator,
            IVolumeWriter volumeWriter)
        {
            _fileFactory = fileFactory;
            _allocator = allocator;
            _volumeWriter = volumeWriter;
        }

        public async Task<IFile> CreateFile(string name)
        {
            if (name.Length > GlobalConstant.MaxFileNameSize)
            {
                throw new FileSystemException($"File name cannot be greater then {GlobalConstant.MaxFileNameSize} symbols");
            }
            
            var fileMetaBlock = new FileMetaBlock();
            var reservedBlocksNumbers = _allocator.AllocateBlocks(1);
            await _volumeWriter.WriteBlocks(fileMetaBlock.Serialize(), reservedBlocksNumbers);
            return _fileFactory.New(fileMetaBlock, name);
        } 
    }
}