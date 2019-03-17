using System;

namespace Vfs
{
    [Serializable]
    public class BlockRun
    {
        public uint AllocationGroup { get; set; }
        
        public uint Start { get; set; }
        
        public uint Length { get; set; }
    }
}