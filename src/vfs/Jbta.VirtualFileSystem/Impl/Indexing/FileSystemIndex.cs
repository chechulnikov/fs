namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class FileSystemIndex
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly BPlusTree _tree;
        
        public FileSystemIndex(FileSystemMeta fileSystemMeta)
        {
            _fileSystemMeta = fileSystemMeta;
            var nodesFactory = new BPlusTreeNodesFactory(_fileSystemMeta);
            _tree = new BPlusTree(nodesFactory);
        }
    }
}