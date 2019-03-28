using System.Threading.Tasks;

namespace Vfs
{
    internal class FileCreator
    {
        private readonly FileFactory _fileFactory;
        private readonly TransactionManager _transactionManager;
        private readonly Allocator _allocator;

        public FileCreator(
            FileFactory fileFactory,
            TransactionManager transactionManager,
            Allocator allocator)
        {
            _fileFactory = fileFactory;
            _transactionManager = transactionManager;
            _allocator = allocator;
        }

        public async Task<IFile> CreateFile(string name)
        {
            using (_transactionManager.StartTransaction())
            {
                var fileMetaBlock = new FileMetaBlock();
                var bytesCount = await _allocator.Allocate(fileMetaBlock.Serialize());
                return _fileFactory.NewFile(fileMetaBlock, name, bytesCount);
            }
        } 
    }
}