namespace Jbta.VirtualFileSystem.Internal
{
    internal static class GlobalConstant
    {
        public const int DefaultBlockSize = 1024;
        public const int SuperblockMagicNumber = 2_137_473_947;
        public const int BitmapBlocksCount = 1024;
        public const int MaxFileDirectBlocksCount = 16;
        public const int MaxFileIndirectBlocksCount = 128;
        public const int MaxFileNameSize = 16;
        public const int MinBPlusTreeDegree = 10;
    }
}