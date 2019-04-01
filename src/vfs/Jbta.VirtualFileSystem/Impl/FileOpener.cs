using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Impl.Indexing;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileOpener
    {
        private readonly FileSystemIndex _fileSystemIndex;
        private readonly FileFactory _fileFactory;
        private readonly IVolumeReader _volumeReader;

        public FileOpener(
            FileSystemIndex fileSystemIndex,
            FileFactory fileFactory,
            IVolumeReader volumeReader)
        {
            _fileSystemIndex = fileSystemIndex;
            _fileFactory = fileFactory;
            _volumeReader = volumeReader;
        }

        public async Task<IFile> Open(string fileName)
        {
            var (fileMetaBlockNumber, hasBeenFound) = _fileSystemIndex.Search(fileName);
            if (!hasBeenFound)
            {
                throw new FileSystemException($"File \"{fileName}\" not found");
            }
            
            var fileMetaBlockData = await _volumeReader.ReadBlocks(fileMetaBlockNumber);
            var fileMetaBlock = FileMetaBlock.Deserialize(fileMetaBlockData);
            return _fileFactory.New(fileMetaBlock, fileName);
        }
    }
}