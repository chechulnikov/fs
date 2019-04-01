using System;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Impl.Indexing;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileOpener
    {
        private readonly FileSystemIndex _fileSystemIndex;

        public FileOpener(FileSystemIndex fileSystemIndex)
        {
            _fileSystemIndex = fileSystemIndex;
        }

        public Task<IFile> Open(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}