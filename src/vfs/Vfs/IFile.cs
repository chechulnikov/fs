using System;
using System.Threading.Tasks;

namespace Vfs
{
    public interface IFile
    {
        Guid Id { get; }
        
        string Name { get; }
        
        int Size { get; }

        Task<byte[]> Read(int offset, int length);
       
        void Write(int offset, byte[] data);
    }
}