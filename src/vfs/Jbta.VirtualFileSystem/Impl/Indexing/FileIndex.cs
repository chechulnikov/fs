namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class FileIndex
    {
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly BPlusTree _tree;
        
        public FileIndex(IFileSystemMeta fileSystemMeta)
        {
            _fileSystemMeta = fileSystemMeta;
            var nodesFactory = new BPlusTreeNodesFactory(_fileSystemMeta);
            _tree = new BPlusTree(nodesFactory);
        }
    }
}