namespace Jbta.VirtualFileSystem.Impl
{
    internal static class GlobalConstant
    {
        public const int SuperblockMagicNumber = 2_137_473_947;

        public const int BitmapBlocksCount = 1024;

        public const int FileDirectBlocksCount = 16;

        public const int FileIndirectBlocksCount = 1024;
    }
}