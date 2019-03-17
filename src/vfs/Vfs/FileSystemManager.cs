using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vfs.Initialization;
using Vfs.Mounting;

namespace Vfs
{
    public class FileSystemManager
    {
        private static readonly object _lock;
        private static readonly FileSystemManager Instance;
        private readonly ConcurrentDictionary<string, IFileSystem> _mountedFileSystems;
        private readonly Mounter _mounter;
        private readonly Initializer _initializer;
        
        static FileSystemManager()
        {
            _lock = new object();
            
            if (Instance != null) return;
            lock (_lock)
                if (Instance == null) Instance = new FileSystemManager();
        }

        private FileSystemManager()
        {
            _mountedFileSystems = new ConcurrentDictionary<string, IFileSystem>();
            _mounter = new Mounter();
            _initializer = new Initializer();
        }

        public static IReadOnlyDictionary<string, IFileSystem> MountedFileSystems => Instance._mountedFileSystems;

        public static Task Init(FileSystemSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            return Instance._initializer.Initialize(settings);
        }

        public static IFileSystem Mount(string volumePath)
        {
            if (string.IsNullOrWhiteSpace(volumePath))
                throw new ArgumentException("Volume path cannot be null or whitespace");
            
            return Instance._mountedFileSystems.GetOrAdd(volumePath, dfp => Instance._mounter.Mount(dfp));
        }

        public static void Unmount(string volumePath)
        {
            if (string.IsNullOrWhiteSpace(volumePath))
                throw new ArgumentException("Volume path cannot be null or whitespace");

            if (Instance._mountedFileSystems.TryRemove(volumePath, out var fileSystem)) fileSystem.Dispose();
        }
    }
}