using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Mounting;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileSystem : IFileSystem
    {
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly FileCreator _fileCreator;
        private readonly Unmounter _unmounter;

        public FileSystem(string volumePath, IFileSystemMeta fileSystemMeta, FileCreator fileCreator, Unmounter unmounter)
        {
            VolumePath = volumePath;
            _fileSystemMeta = fileSystemMeta;
            _fileCreator = fileCreator;
            _unmounter = unmounter;
            IsMounted = true;
        }
        
        public string VolumePath { get; }

        public ulong VolumeSize => (ulong) (_fileSystemMeta.BlockSize * _fileSystemMeta.BlocksCount);
        
        public ulong UsedSpace => (ulong) (_fileSystemMeta.BlockSize * _fileSystemMeta.UsedBlocksCount);

        public ulong UnusedSpace => VolumeSize - UsedSpace;
        
        public bool IsMounted { get; private set; }

        public Task<IFile> CreateFile(string fileName) => _fileCreator.CreateFile(fileName);

        public void DeleteFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public IFile OpenFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _unmounter.Unmount().Wait();
            IsMounted = false;
        }
    }
}