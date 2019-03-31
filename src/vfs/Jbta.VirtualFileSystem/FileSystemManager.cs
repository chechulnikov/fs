using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Initialization;
using Jbta.VirtualFileSystem.Mounting;

namespace Jbta.VirtualFileSystem
{
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
                if (_instance == null) _instance = new FileSystemManager();
        }

        private FileSystemManager()
        {
            _mountedFileSystems = new Dictionary<string, IFileSystem>();
            _mounter = new Mounter();
            _initializer = new Initializer();
        }

        public static IReadOnlyDictionary<string, IFileSystem> MountedFileSystems => _instance._mountedFileSystems;

        public static Task Init(FileSystemSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            return Task.FromResult(_instance._initializer.Initialize(settings));
        }

        // TODO to async?
        public static IFileSystem Mount(string volumePath)
        {
            if (string.IsNullOrWhiteSpace(volumePath))
                throw new ArgumentException("Volume path cannot be null or whitespace");

            if (_instance._mountedFileSystems.ContainsKey(volumePath))
                return _instance._mountedFileSystems[volumePath];
            lock (Locker)
            {
                if (_instance._mountedFileSystems.ContainsKey(volumePath))
                    return _instance._mountedFileSystems[volumePath];
                
                var fileSystem = _instance._mounter.Mount(volumePath).Result;
                _instance._mountedFileSystems.Add(volumePath, fileSystem);
                return fileSystem;
            }
        }

        public static void Unmount(IFileSystem fileSystem)
        {
            if (fileSystem == null) throw new ArgumentNullException(nameof(fileSystem));
            
            if (!_instance._mountedFileSystems.ContainsKey(fileSystem.VolumePath)) return;
            lock (Locker)
            {
                if (!_instance._mountedFileSystems.ContainsKey(fileSystem.VolumePath)) return;
                if (_instance._mountedFileSystems.Remove(fileSystem.VolumePath, out var fs)) fs.Dispose();
            }
        }
    }
}