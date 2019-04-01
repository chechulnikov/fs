namespace Jbta.VirtualFileSystem.Internal.Blocks
{
    public class Superblock
    {
        public int MagicNumber { get; set; }
        
        public bool IsDirty { get; set; }
        
        public int BlockSize { get; set; }
        
        public int RootIndexBlockNumber { get; set; }
    }
}