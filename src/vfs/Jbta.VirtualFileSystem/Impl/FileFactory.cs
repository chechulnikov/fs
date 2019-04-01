namespace Jbta.VirtualFileSystem.Impl
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
        
        public IFile New(FileMetaBlock fileMetaBlock, string name, int size)
        {
            return new File(_fileReader, _writer, fileMetaBlock, name, size);
        }
    }
}