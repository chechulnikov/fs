using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks.Serialization;
using Jbta.VirtualFileSystem.Internal.Initialization;
using Jbta.VirtualFileSystem.Internal.Mounting;

namespace Jbta.VirtualFileSystem
{
    /// <summary>
    /// This is static API, that provides a possibilities to create, mount and unmount file systems
    /// </summary>
    public class FileSystemManager
    {
        private static readonly object Locker;
        private static volatile FileSystemManager _instance;
        private readonly Dictionary<string, IFileSystem> _mountedFileSystems;
        private readonly Mounter _mounter;
        private readonly Initializer _initializer;
        
        static FileSystemManager()
        {
            Locker = new object();
            
            if (_instance != null) return;
            lock (Locker)
            {
                if (_instance == null)
                {
                    _instance = new FileSystemManager();
                }
            }
        }

        private FileSystemManager()
        {
            _mountedFileSystems = new Dictionary<string, IFileSystem>();
            var superblockSerializer = new SuperblockSerializer(GlobalConstant.BlockSize);
            var indexBlockSerializer = new IndexBlockSerializer(GlobalConstant.BlockSize);
            _mounter = new Mounter(superblockSerializer, indexBlockSerializer);
            _initializer = new Initializer(superblockSerializer, indexBlockSerializer);
        }

        /// <summary>
        /// List of mounted file systems
        /// </summary>
        public static IReadOnlyDictionary<string, IFileSystem> MountedFileSystems => _instance._mountedFileSystems;

        /// <summary>
        /// Initialize new file system in volume by given file path
        /// </summary>
        /// <param name="volumePath">File path to file where will created new file system</param>
        public static Task Init(string volumePath)
        {
            volumePath = volumePath?.Trim();
            if (string.IsNullOrWhiteSpace(volumePath))
                throw new ArgumentException("Volume path cannot be null or whitespace", nameof(volumePath));

            return _instance._initializer.Initialize(volumePath).AsTask();
        }

        /// <summary>
        /// Mounts existed file system to current process
        /// </summary>
        /// <param name="volumePath">Path to file with valid volume</param>
        /// <returns>An object, that represents a file system</returns>
        /// <exception cref="ArgumentException">File path should be valid</exception>
        public static IFileSystem Mount(string volumePath)
        {
            volumePath = volumePath?.Trim();
            if (!System.IO.File.Exists(volumePath))
                throw new ArgumentException($"File not found by path {volumePath}");

            if (_instance._mountedFileSystems.ContainsKey(volumePath))
                return _instance._mountedFileSystems[volumePath];
            lock (Locker)
            {
                if (_instance._mountedFileSystems.ContainsKey(volumePath))
                    return _instance._mountedFileSystems[volumePath];
                
                var fileSystem = _instance._mounter.Mount(volumePath).Result; // TODO to async?
                _instance._mountedFileSystems.Add(volumePath, fileSystem);
                return fileSystem;
            }
        }

        /// <summary>
        /// Unmount already mounted file system from process.
        /// If file system isn't mounted nothing will happened
        /// </summary>
        /// <param name="volumePath">Path to file with valid volume</param>
        /// <exception cref="ArgumentNullException">fileSystem object is null</exception>
        public static void Unmount(string volumePath)
        {
            if (!System.IO.File.Exists(volumePath))
                throw new ArgumentException($"File not found by path {volumePath}");
            
            if (!_instance._mountedFileSystems.ContainsKey(volumePath)) return;
            lock (Locker)
            {
                if (!_instance._mountedFileSystems.ContainsKey(volumePath)) return;
                if (_instance._mountedFileSystems.Remove(volumePath, out var fs)) fs.Dispose();
            }
        }
    }
}