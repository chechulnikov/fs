using System.Collections.Generic;

namespace Jbta.VirtualFileSystem.Impl
{
    internal struct AllocationResult
    {
        public IReadOnlyList<int> ReservedBlocks { get; set; }
        
        public int BytesAllocated { get; set; }
    }
}