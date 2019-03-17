using Vfs.Utils;

namespace Vfs
{
    public class FileSystemSettings
    {   
        public string VolumePath { get; set; }

        public ushort BlocksCountPerAllocationGroup { get; set; } = 8197;
        
        // 1024, 2048, 4096 or 8192 bytes only
        public ushort BlockSize { get; set; } = (ushort) 1.KiB();
    }
}