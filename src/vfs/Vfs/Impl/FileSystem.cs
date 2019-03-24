namespace Vfs
{
    internal class FileSystem : IFileSystem
    {
        private readonly Volume _volume;
        private readonly Superblock _superblock;

        public FileSystem(string volumePath, Volume volume, Superblock superblock)
        {
            VolumePath = volumePath;
            _volume = volume;
            _superblock = superblock;
        }
        
        public string VolumePath { get; }
        public ulong VolumeSize { get; }
        public ulong UsedSpace { get; }
        public ulong UnusedSpace { get; }

        public IDirectory Root { get; }
        
        public void CreateFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public IFile OpenFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}