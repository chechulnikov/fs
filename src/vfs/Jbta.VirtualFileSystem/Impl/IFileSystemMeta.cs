namespace Jbta.VirtualFileSystem.Impl
{
    internal interface IFileSystemMeta
    {
        int BlockSize { get; }
        
        int BlocksCount { get; }
        
        int UsedBlocksCount { get; }
    }
}