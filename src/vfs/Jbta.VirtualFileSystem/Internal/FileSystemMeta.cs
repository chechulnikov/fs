using Jbta.VirtualFileSystem.Internal.SpaceManagement;

namespace Jbta.VirtualFileSystem.Internal
{
    internal class FileSystemMeta
    {
        private readonly Bitmap _bitmap;

        public FileSystemMeta(int blockSize, Bitmap bitmap)
        {
            _bitmap = bitmap;
            BlockSize = blockSize;
        }
        
        public int BlockSize { get; }

        public int BlocksCount => GlobalConstant.BitmapBlocksCount * BlockSize * 8;

        public int UsedBlocksCount => _bitmap.SetBitsCount;
    }
}