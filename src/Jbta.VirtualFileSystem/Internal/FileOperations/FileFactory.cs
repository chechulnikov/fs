using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;

namespace Jbta.VirtualFileSystem.Internal.FileOperations
{
    internal class FileFactory
    {
        private readonly FileReader _fileReader;
        private readonly FileWriter _writer;
        private readonly FileSizeMeter _sizeMeter;

        public FileFactory(
            FileReader fileReader,
            FileWriter writer,
            FileSizeMeter sizeMeter)
        {
            _fileReader = fileReader;
            _writer = writer;
            _sizeMeter = sizeMeter;
        }
        
        public IFile New(FileMetaBlock fileMetaBlock, string name) =>
            new File(_fileReader, _writer, _sizeMeter, fileMetaBlock, name);
    }
}