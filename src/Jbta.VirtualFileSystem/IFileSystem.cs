using System;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem
{
    /// <summary>
    /// Object, that represents a file system
    /// </summary>
    public interface IFileSystem : IDisposable
    {
        /// <summary>
        /// Path to file that contains a file system data
        /// </summary>
        string VolumePath { get; }
        
        /// <summary>
        /// Maximum volume size in bytes
        /// </summary>
        ulong VolumeSize { get; }
        
        /// <summary>
        /// Already used space of volume in bytes
        /// </summary>
        ulong UsedSpace { get; }
        
        /// <summary>
        /// Free space of volume in bytes
        /// </summary>
        ulong UnusedSpace { get; }
        
        /// <summary>
        /// Indicates that the file system mounted or not
        /// </summary>
        bool IsMounted { get; }

        /// <summary>
        /// Creates file by given file name
        /// </summary>
        /// <param name="fileName">A unique name of the file in file system</param>
        /// <exception cref="ArgumentException">Invalid file name</exception>
        /// <exception cref="FileSystemException">File system isn't mounted</exception>
        /// <exception cref="FileSystemException">File has already existed</exception>
        Task CreateFile(string fileName);

        /// <summary>
        /// Deletes file by given file name
        /// </summary>
        /// <param name="fileName">A unique name of the file in file system</param>
        /// <returns>False means file cannot be deleted because it is opened</returns>
        Task<bool> DeleteFile(string fileName);
        
        /// <summary>
        /// Opens file by given file name
        /// </summary>
        /// <param name="fileName">A unique name of the file in file system</param>
        /// <returns></returns>
        Task<IFile> OpenFile(string fileName);

        /// <summary>
        /// Closes file 
        /// </summary>
        /// <param name="file">Object, that represents a file in file system</param>
        /// <returns>Boolean value, that indicates that file was closed of not</returns>
        bool CloseFile(IFile file);
    }
}