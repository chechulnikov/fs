using System.Collections.Generic;

namespace Vfs
{
    internal struct AllocationResult
    {
        public IReadOnlyList<int> ReservedBlocks { get; set; }
        
        public int BytesAllocated { get; set; }
    }
}