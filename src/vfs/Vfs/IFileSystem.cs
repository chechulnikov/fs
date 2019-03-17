using System;

namespace Vfs
{
    public interface IFileSystem : IDisposable
    {
        string VolumePath { get; }
        ulong VolumeSize { get; }
        ulong UsedSpace { get; }
        ulong UnusedSpace { get; }

        void CreateFile(string path);
        void DeleteFile(string path);
        IFile OpenFile(string path);
        
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        IDirectory OpenDirectory(string path);
        
        IDirectory Root { get; }
    }
}