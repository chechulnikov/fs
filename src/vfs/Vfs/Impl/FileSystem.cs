namespace Vfs
{
    internal class FileSystem : IFileSystem
    {
        public FileSystem(string deviceFilePath)
        {
            DeviceFilePath = deviceFilePath;
        }
        
        public string DeviceFilePath { get; }
        
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