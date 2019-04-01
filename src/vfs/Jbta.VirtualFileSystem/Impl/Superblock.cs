using System;

namespace Jbta.VirtualFileSystem.Impl
{
    [Serializable]
    public class Superblock
    {
        public int MagicNumber { get; set; }
        
        public bool IsDirty { get; set; }
        
        public int BlockSize { get; set; }
        
        public int RootIndexBlockNumber { get; set; }
    }
}