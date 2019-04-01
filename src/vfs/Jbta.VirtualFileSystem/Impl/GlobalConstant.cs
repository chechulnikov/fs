namespace Jbta.VirtualFileSystem.Impl
{
    internal static class GlobalConstant
    {
        public const int SuperblockMagicNumber = 2_137_473_947;

        public const int BitmapBlocksCount = 1024;

        public const int MaxFileDirectBlocksCount = 16;

        public const int MaxFileIndirectBlocksCount = 1024;

        // 16 symbols
        public const int MaxFileNameSize = 16;
        
        public const int MaxFileNameSizeInBytes = MaxFileNameSize * 2;

        public const int BPlusTreeDegree = 10;
    }
}