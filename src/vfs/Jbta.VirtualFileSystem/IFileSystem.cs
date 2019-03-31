using System;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem
{
    public interface IFileSystem : IDisposable
    {
        string VolumePath { get; }
        
        ulong VolumeSize { get; }
        
        ulong UsedSpace { get; }
        
        ulong UnusedSpace { get; }
        
        bool IsMounted { get; }

        Task<IFile> CreateFile(string path);
        
        void DeleteFile(string path);
        
        IFile OpenFile(string path);
    }
}