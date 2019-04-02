namespace Jbta.VirtualFileSystem.Internal.DataAccess.Blocks
{
    public class Superblock
    {
        public int MagicNumber { get; set; }
        
        public bool IsDirty { get; set; }
        
        public int BlockSize { get; set; }
        
        public int RootIndexBlockNumber { get; set; }
    }
}