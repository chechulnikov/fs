using System;

namespace Jbta.VirtualFileSystem.Exceptions
{
    public class FileSystemException : Exception
    {
        public FileSystemException(string message)
        {
            Message = message;
        }

        public override string Message { get; }
    }
}