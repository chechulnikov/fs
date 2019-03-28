using System;
using System.Threading.Tasks;

namespace Vfs
{
    public interface IFileSystem : IDisposable
    {
        string VolumePath { get; }
        ulong VolumeSize { get; }
        ulong UsedSpace { get; }
        ulong UnusedSpace { get; }

        Task<IFile> CreateFile(string path);
        void DeleteFile(string path);
        IFile OpenFile(string path);
        
        IDirectory Root { get; }
    }
}