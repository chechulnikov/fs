using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vfs.Initialization;
using Vfs.Mounting;

namespace Vfs
{
    public class FileSystemManager
    {
        private static readonly object _lock;
        private static readonly FileSystemManager _instance;
        private readonly ConcurrentDictionary<string, IFileSystem> _mountedFileSystems;
        private readonly Mounter _mounter;
        private readonly Initializer _initializer;
        
        static FileSystemManager()
        {
            _lock = new object();
            
            if (_instance != null) return;
            lock (_lock)
                if (_instance == null) _instance = new FileSystemManager();
        }

        private FileSystemManager()
        {
            _mountedFileSystems = new ConcurrentDictionary<string, IFileSystem>();
            _mounter = new Mounter();
            _initializer = new Initializer();
        }

        public static IReadOnlyDictionary<string, IFileSystem> MountedFileSystems => _instance._mountedFileSystems;

        public static void Init(string deviceFilePath)
        {
            if (string.IsNullOrWhiteSpace(deviceFilePath))
                throw new ArgumentException("Device file path cannot be null or whitespace");
            
            _instance._initializer.Initialize(deviceFilePath);
        }

        public static IFileSystem Mount(string deviceFilePath)
        {
            if (string.IsNullOrWhiteSpace(deviceFilePath))
                throw new ArgumentException("Device file path  cannot be null or whitespace");

            return _instance._mountedFileSystems.GetOrAdd(deviceFilePath, dfp => _instance._mounter.Mount(dfp));
        }

        public static void Unmount(string deviceFilePath)
        {
            if (string.IsNullOrWhiteSpace(deviceFilePath))
                throw new ArgumentException("Device file path  cannot be null or whitespace");

            if (_instance._mountedFileSystems.TryRemove(deviceFilePath, out var fileSystem)) fileSystem.Dispose();
        }
    }
}