using System;
using System.Threading;
using System.Threading.Tasks;
using Vfs.Utils;

namespace Vfs
{
    internal class File : IFile
    {
        private readonly FileReader _reader;
        private readonly FileMetaBlock _fileMetaBlock;
        private readonly int _blockSize;
        private readonly ReaderWriterLockSlim _locker;

        public File(FileReader reader, FileMetaBlock fileMetaBlock, string name, int blockSize)
        {
            _reader = reader;
            _fileMetaBlock = fileMetaBlock;
            _blockSize = blockSize;
            Name = name;
            _locker = new ReaderWriterLockSlim();
        }

        public Guid Id => _fileMetaBlock.FileId;

        public string Name { get; }

        public int Size
        {
            get
            {
                var directBlocksCount = _fileMetaBlock.DirectBlocks.Length;
                var indirectBlocksCount = _fileMetaBlock.IndirectBlocks.Length;
                var indirectBlockCapacity = _blockSize / sizeof(int);
                return (directBlocksCount + indirectBlocksCount * indirectBlockCapacity) * _blockSize;
            }
        }

        public Task<byte[]> Read(int offset, int length)
        {
            using (_locker.ReaderLock())
            {
                return _reader.Read(_fileMetaBlock, offset, length);
            }
        }

        public void Write(int offset, byte[] data)
        {
            using (_locker.WriterLock())
            {
                throw new System.NotImplementedException();
            }
        }
    }
}