using System;

namespace Jbta.VirtualFileSystem.Impl
{
    [Serializable]
    public class Superblock : IFileSystemMeta
    {
        public int MagicNumber { get; set; }
        
        public bool IsDirty { get; set; }
        
        public int BlockSize { get; set; }
        
        public int BlocksCount { get; set; }
        
        public int UsedBlocksCount { get; set; }
    }
}