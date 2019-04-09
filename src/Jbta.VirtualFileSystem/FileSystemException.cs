using System;

namespace Jbta.VirtualFileSystem
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