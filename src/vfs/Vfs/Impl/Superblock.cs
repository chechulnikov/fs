using System;

namespace Vfs
{
    [Serializable]
    public class Superblock
    {
        public uint BlockSize { get; set; }
        
        public uint BlocksCount { get; set; }
        
        public uint UsedBlocksCount { get; set; }
        
        public uint INodeSize { get; set; }
        
        public uint BlocksCountPerAllocationGroup { get; set; }
        
        public uint AllocationGroupsCount { get; set; }
        
        public BlockRun RootInode { get; set; }
    }
}