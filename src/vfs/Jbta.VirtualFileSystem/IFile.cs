using System;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem
{
    public interface IFile : IDisposable
    {
        string Name { get; }
        
        int Size { get; }

        Task<Memory<byte>> Read(int offset, int length);
       
        Task Write(int offset, byte[] data);
    }
}