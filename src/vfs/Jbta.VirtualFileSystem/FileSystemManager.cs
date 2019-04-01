using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
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

        public static Task Init(string volumePath)
        {
            if (!System.IO.File.Exists(volumePath))
                throw new ArgumentException($"File not found by path {volumePath}");
            
            return Task.FromResult(_instance._initializer.Initialize(volumePath));
        }

        // TODO to async?
        public static IFileSystem Mount(string volumePath)
        {
            if (!System.IO.File.Exists(volumePath)) throw new VolumeNotFoundException(volumePath);

            if (_instance._mountedFileSystems.ContainsKey(volumePath))
                return _instance._mountedFileSystems[volumePath];
            lock (Locker)
            {
                if (_instance._mountedFileSystems.ContainsKey(volumePath))
                    return _instance._mountedFileSystems[volumePath];
                
                var fileSystem = Mounter.Mount(volumePath).Result;
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