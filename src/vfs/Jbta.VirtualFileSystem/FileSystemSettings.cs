namespace Jbta.VirtualFileSystem
{
    public class FileSystemSettings
    {   
        public string VolumePath { get; set; }
        
        // 1024, 2048, 4096 or 8192 bytes only
        public int BlockSize { get; set; } = DefaultSettings.BlockSize;
    }
}