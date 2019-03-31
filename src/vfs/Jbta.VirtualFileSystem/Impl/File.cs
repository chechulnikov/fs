using System;
using System.Threading;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class File : IFile
    {
        private readonly FileReader _reader;
        private readonly FileWriter _writer;
        private readonly FileMetaBlock _fileMetaBlock;
        private readonly int _blockSize;
        private readonly ReaderWriterLockSlim _locker;

        public File(FileReader reader, FileWriter writer, FileMetaBlock fileMetaBlock, string name, int blockSize)
        {
            _reader = reader;
            _writer = writer;
            _fileMetaBlock = fileMetaBlock;
            _blockSize = blockSize;
            Name = name;
            _locker = new ReaderWriterLockSlim();
        }

        public Guid Id => _fileMetaBlock.FileId;

        public string Name { get; }

        public int Size => _fileMetaBlock.CalcDataBlocksSizeInBytes(_blockSize) + _blockSize;

        public Task<byte[]> Read(int offset, int length)
        {
            using (_locker.ReaderLock())
            {
                return _reader.Read(_fileMetaBlock, offset, length);
            }
        }

        public Task Write(int offset, byte[] data)
        {
            using (_locker.WriterLock())
            {
                return _writer.Write(_fileMetaBlock, offset, data);
            }
        }
    }
}