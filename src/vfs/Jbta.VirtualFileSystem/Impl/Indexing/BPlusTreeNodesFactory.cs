namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class BPlusTreeNodesFactory
    {
        private readonly FileSystemMeta _fileSystemMeta;

        public BPlusTreeNodesFactory(FileSystemMeta fileSystemMeta)
        {
            _fileSystemMeta = fileSystemMeta;
        }
        
        public IBPlusTreeNode New() => new IndexBlock(_fileSystemMeta.BlockSize);

        public IBPlusTreeNode From(byte[] block) => IndexBlock.Deserialize(block);
    }
}