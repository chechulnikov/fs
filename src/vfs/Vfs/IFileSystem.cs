using System;

namespace Vfs
{
    public interface IFileSystem : IDisposable
    {
        string DeviceFilePath { get; }
        IDirectory Root { get; }
        
        void CreateFile(string path);
        void DeleteFile(string path);
        IFile OpenFile(string path);
        
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        IDirectory OpenDirectory(string path);
    }
}