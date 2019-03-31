namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class BPlusTreeNodesFactory
    {
        private readonly IFileSystemMeta _fileSystemMeta;

        public BPlusTreeNodesFactory(IFileSystemMeta fileSystemMeta)
        {
            _fileSystemMeta = fileSystemMeta;
        }
        
        public IBPlusTreeNode New()
        {
            return new IndexBlock(_fileSystemMeta.BlockSize);
        }
    }
}