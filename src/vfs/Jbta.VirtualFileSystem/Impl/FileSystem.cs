using System.Threading.Tasks;

namespace Vfs
{
    internal class FileSystem : IFileSystem
    {
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly FileCreator _fileCreator;

        public FileSystem(string volumePath, IFileSystemMeta fileSystemMeta, FileCreator fileCreator)
        {
            VolumePath = volumePath;
            _fileSystemMeta = fileSystemMeta;
            _fileCreator = fileCreator;
        }
        
        public string VolumePath { get; }

        public ulong VolumeSize => (ulong) (_fileSystemMeta.BlockSize * _fileSystemMeta.BlocksCount);
        
        public ulong UsedSpace => (ulong) (_fileSystemMeta.BlockSize * _fileSystemMeta.UsedBlocksCount);

        public ulong UnusedSpace => VolumeSize - UsedSpace;

        public IDirectory Root { get; }
        
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
            throw new System.NotImplementedException();
        }
    }
}