namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class FileSystemIndex
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly BPlusTree _tree;
        
        public FileSystemIndex(FileSystemMeta fileSystemMeta, byte[] rootIndexBlock)
        {
            _fileSystemMeta = fileSystemMeta;
            var nodesFactory = new BPlusTreeNodesFactory(_fileSystemMeta);
            var root = nodesFactory.From(rootIndexBlock);
            _tree = new BPlusTree(nodesFactory, root);
        }

        public int Search(string fileName) => _tree.Search(fileName);
    }
}