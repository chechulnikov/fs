using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.Indexing;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileOpener
    {
        private readonly FileSystemIndex _fileSystemIndex;
        private readonly FileFactory _fileFactory;
        private readonly IBinarySerializer<FileMetaBlock> _fileMetaBlockSerializer;
        private readonly IVolumeReader _volumeReader;

        public FileOpener(
            FileSystemIndex fileSystemIndex,
            FileFactory fileFactory,
            IBinarySerializer<FileMetaBlock> fileMetaBlockSerializer,
            IVolumeReader volumeReader)
        {
            _fileSystemIndex = fileSystemIndex;
            _fileFactory = fileFactory;
            _fileMetaBlockSerializer = fileMetaBlockSerializer;
            _volumeReader = volumeReader;
        }

        public async Task<IFile> Open(string fileName)
        {
            var (fileMetaBlockNumber, hasBeenFound) = _fileSystemIndex.SearchFile(fileName);
            if (!hasBeenFound)
            {
                throw new FileSystemException($"File \"{fileName}\" not found");
            }
            
            var fileMetaBlockData = await _volumeReader.ReadBlocks(fileMetaBlockNumber);
            var fileMetaBlock = _fileMetaBlockSerializer.Deserialize(fileMetaBlockData);
            return _fileFactory.New(fileMetaBlock, fileName);
        }
    }
}