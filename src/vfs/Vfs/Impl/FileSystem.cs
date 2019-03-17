namespace Vfs
{
    internal class FileSystem : IFileSystem
    {
        public FileSystem(string deviceFilePath)
        {
            VolumePath = deviceFilePath;
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

        public void CreateDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public IDirectory OpenDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}