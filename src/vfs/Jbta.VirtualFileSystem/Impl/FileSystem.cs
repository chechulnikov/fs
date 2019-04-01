using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Mounting;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileSystem : IFileSystem
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly FileCreator _fileCreator;
        private readonly FileOpener _fileOpener;
        private readonly Unmounter _unmounter;

        public FileSystem(
            string volumePath,
            FileSystemMeta fileSystemMeta,
            FileCreator fileCreator,
            FileOpener fileOpener,
            Unmounter unmounter)
        {
            VolumePath = volumePath;
            _fileSystemMeta = fileSystemMeta;
            _fileCreator = fileCreator;
            _fileOpener = fileOpener;
            _unmounter = unmounter;
            IsMounted = true;
        }

        private int BlocksCount => _fileSystemMeta.BlockSize * GlobalConstant.BitmapBlocksCount * 8;
        
        public string VolumePath { get; }

        public ulong VolumeSize => (ulong) (_fileSystemMeta.BlockSize * BlocksCount);
        
        public ulong UsedSpace => (ulong) (_fileSystemMeta.BlockSize * _fileSystemMeta.UsedBlocksCount);

        public ulong UnusedSpace => VolumeSize - UsedSpace;
        
        public bool IsMounted { get; private set; }

        public Task<IFile> CreateFile(string fileName) => _fileCreator.CreateFile(fileName);

        public void DeleteFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<IFile> OpenFile(string path) => _fileOpener.Open(path);

        public void Dispose()
        {
            _unmounter.Unmount().Wait();
            IsMounted = false;
        }
    }
}