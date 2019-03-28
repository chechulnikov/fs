namespace Vfs
{
    internal class FileFactory
    {
        private readonly FileReader _fileReader;

        public FileFactory(FileReader fileReader)
        {
            _fileReader = fileReader;
        }
        
        public IFile NewFile(FileMetaBlock fileMetaBlock, string name, int size)
        {
            return new File(_fileReader, fileMetaBlock, name, size);
        }
    }
}