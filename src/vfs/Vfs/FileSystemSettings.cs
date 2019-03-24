using Vfs.Utils;

namespace Vfs
{
    public class FileSystemSettings
    {   
        public string VolumePath { get; set; }

        public int BlocksCountPerAllocationGroup { get; set; } = Default.BlocksCountPerAllocationGroup;
        
        // 1024, 2048, 4096 or 8192 bytes only
        public int BlockSize { get; set; } = Default.BlockSize;
    }
}