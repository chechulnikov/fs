namespace Vfs
{
    internal class FileFactory
    {
        private readonly FileReader _fileReader;
        private readonly FileWriter _writer;

        public FileFactory(FileReader fileReader, FileWriter writer)
        {
            _fileReader = fileReader;
            _writer = writer;
        }
        
        public IFile NewFile(FileMetaBlock fileMetaBlock, string name, int size)
        {
            return new File(_fileReader, _writer, fileMetaBlock, name, size);
        }
    }
}