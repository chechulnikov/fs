using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileFactory
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly FileReader _fileReader;
        private readonly FileWriter _writer;

        public FileFactory(FileSystemMeta fileSystemMeta, FileReader fileReader, FileWriter writer)
        {
            _fileSystemMeta = fileSystemMeta;
            _fileReader = fileReader;
            _writer = writer;
        }
        
        public IFile New(FileMetaBlock fileMetaBlock, string name) =>
            new File(_fileSystemMeta, _fileReader, _writer, fileMetaBlock, name);
    }
}