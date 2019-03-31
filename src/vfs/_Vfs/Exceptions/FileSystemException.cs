using System;

namespace Vfs
{
    public class FileSystemException : Exception
    {
        protected FileSystemException() {}
        
        public FileSystemException(string message)
        {
            Message = message;
        }

        public override string Message { get; }
    }
}