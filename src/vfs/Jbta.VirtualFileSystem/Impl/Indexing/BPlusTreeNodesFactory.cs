namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class BPlusTreeNodesFactory
    {
        private readonly FileSystemMeta _fileSystemMeta;

        public BPlusTreeNodesFactory(FileSystemMeta fileSystemMeta)
        {
            _fileSystemMeta = fileSystemMeta;
        }
        
        public IBPlusTreeNode New()
        {
            return new IndexBlock(_fileSystemMeta.BlockSize);
        }
    }
}