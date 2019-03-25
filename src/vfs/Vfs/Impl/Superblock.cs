using System;

namespace Vfs
{
    [Serializable]
    public class Superblock : IFileSystemMeta
    {
        public int MagicNumber { get; set; }
        
        public int BlockSize { get; set; }
        
        public int BlocksCount { get; set; }
        
        public int UsedBlocksCount { get; set; }
        
        public int INodeSize { get; set; }
        
        public int BlocksCountPerAllocationGroup { get; set; }
        
        public int AllocationGroupsCount { get; set; }
        
        public BlockRun RootInode { get; set; }
    }
}