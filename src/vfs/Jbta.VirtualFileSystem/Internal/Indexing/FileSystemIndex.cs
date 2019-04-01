namespace Jbta.VirtualFileSystem.Internal.Indexing
{
    internal class FileSystemIndex
    {
        private readonly BPlusTree _tree;
        
        public FileSystemIndex(BPlusTreeNodesFactory treeNodesFactory, byte[] rootIndexBlock)
        {
            var root = treeNodesFactory.From(rootIndexBlock);
            _tree = new BPlusTree(treeNodesFactory, root);
        }

        public (int, bool) Search(string fileName) => _tree.Search(fileName);

        public bool Exists(string fileName)
        {
            var (_, hasFileAlreadyExisted) = Search(fileName);
            return hasFileAlreadyExisted;
        }
    }
}