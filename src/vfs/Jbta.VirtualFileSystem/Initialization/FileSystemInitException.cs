namespace Vfs.Initialization
{
    internal class FileSystemInitException : FileSystemException
    {
        private readonly string _message;

        public FileSystemInitException(string message)
        {
            _message = message;
        }

        public override string Message => $"File system initialization error. {_message}";
    }
}