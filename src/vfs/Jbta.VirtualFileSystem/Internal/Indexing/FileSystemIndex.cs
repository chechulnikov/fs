using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.Indexing.DataStructure;

namespace Jbta.VirtualFileSystem.Internal.Indexing
{
    internal class FileSystemIndex
    {
        private readonly BPlusTree _tree;
        
        public FileSystemIndex(
            BPlusTreeNodesPersistenceManager treeNodesPersistenceManager,
            IndexBlock rootIndexBlock,
            int rootIndexBlockNumber)
        {
            var root = treeNodesPersistenceManager.CreateFrom(rootIndexBlock, rootIndexBlockNumber);
            _tree = new BPlusTree(treeNodesPersistenceManager, root);
        }

        public (int, bool) SearchFile(string fileName) => _tree.Search(fileName);

        public bool FileExists(string fileName)
        {
            var (_, hasFileAlreadyExisted) = SearchFile(fileName);
            return hasFileAlreadyExisted;
        }

        public Task<bool> Insert(string fileName, int fileMetaBlockNumber) => _tree.Insert(fileName, fileMetaBlockNumber);

        public Task<bool> RemoveFile(string fileName) => _tree.Delete(fileName);
    }
}