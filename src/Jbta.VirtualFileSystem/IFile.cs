using System;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem
{
    /// <summary>
    /// Object, that represents a file in file system
    /// </summary>
    public interface IFile : IDisposable
    {
        /// <summary>
        /// File name (max 16 characters)
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Size of file in bytes
        /// </summary>
        Task<int> Size { get; }
        
        /// <summary>
        /// True if file has closed
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Method for reading content from the file
        /// </summary>
        /// <param name="offset">Offset in bytes from the beginning of file</param>
        /// <param name="length">Count of bytes, that should be read</param>
        /// <returns>Memory of bytes with file content</returns>
        Task<Memory<byte>> Read(int offset, int length);
       
        /// <summary>
        /// Method for writing content to the file
        /// </summary>
        /// <param name="offset">Offset in bytes from the beginning of file</param>
        /// <param name="data">Byte array of data, that should be written</param>
        Task Write(int offset, byte[] data);
    }
}