namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileSystemMeta
    {
        private readonly BitmapTree _tree;

        public FileSystemMeta(int blockSize, BitmapTree tree)
        {
            _tree = tree;
            BlockSize = blockSize;
        }
        
        public int BlockSize { get; }

        public int BlocksCount => GlobalConstant.BitmapBlocksCount * BlockSize * 8;

        public int UsedBlocksCount => _tree.SetBitsCount;

        public int UnusedBlocksCount => BlocksCount - UsedBlocksCount;
    }
}