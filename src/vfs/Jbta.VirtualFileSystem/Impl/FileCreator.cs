using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileCreator
    {
        private readonly FileFactory _fileFactory;
        private readonly Allocator _allocator;
        private readonly Volume _volume;

        public FileCreator(
            FileFactory fileFactory,
            Allocator allocator,
            Volume volume)
        {
            _fileFactory = fileFactory;
            _allocator = allocator;
            _volume = volume;
        }

        public async Task<IFile> CreateFile(string name)
        {
            var fileMetaBlock = new FileMetaBlock();
            var allocationResult = _allocator.AllocateBlocks(1);
            await _volume.WriteBlocks(fileMetaBlock.Serialize(), allocationResult.ReservedBlocks);
            return _fileFactory.NewFile(fileMetaBlock, name, allocationResult.BytesAllocated);
        } 
    }
}