namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class FileSystemIndex
    {
        private readonly BPlusTree _tree;
        
        public FileSystemIndex(FileSystemMeta fileSystemMeta, byte[] rootIndexBlock)
        {
            var nodesFactory = new BPlusTreeNodesFactory(fileSystemMeta);
            var root = nodesFactory.From(rootIndexBlock);
            _tree = new BPlusTree(nodesFactory, root);
        }

        public (int, bool) Search(string fileName) => _tree.Search(fileName);

        public bool Exists(string fileName)
        {
            var (_, hasFileAlreadyExisted) = Search(fileName);
            return hasFileAlreadyExisted;
        }
    }
}