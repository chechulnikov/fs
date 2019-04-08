using System;
using System.Threading;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.FileOperations;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal
{
    internal class File : IFile
    {
        private readonly FileReader _reader;
        private readonly FileWriter _writer;
        private readonly FileSizeMeter _sizeMeter;
        private readonly FileMetaBlock _fileMetaBlock;
        private readonly ReaderWriterLockSlim _locker;

        public File(
            FileReader reader,
            FileWriter writer,
            FileSizeMeter sizeMeter,
            FileMetaBlock fileMetaBlock,
            string name)
        {
            _reader = reader;
            _writer = writer;
            _sizeMeter = sizeMeter;
            _fileMetaBlock = fileMetaBlock;
            Name = name;
            _locker = new ReaderWriterLockSlim();
        }

        public string Name { get; }

        public Task<int> Size
        {
            get
            {
                if (IsClosed)
                {
                    throw new FileSystemException("Cannot get size info from closed file");
                }

                using (_locker.ReaderLock())
                {
                    return _sizeMeter.MeasureSize(_fileMetaBlock);
                }
            }
        }

        public bool IsClosed { get; private set; }

        public Task<Memory<byte>> Read(int offset, int length)
        {
            if (IsClosed) throw new FileSystemException("Cannot read from closed file");
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            using (_locker.ReaderLock())
            {
                return _reader.Read(_fileMetaBlock, offset, length);
            }
        }

        public Task Write(int offset, byte[] data)
        {
            if (IsClosed) throw new FileSystemException("Cannot write to closed file");
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (data == null || data.Length == 0) throw new ArgumentException(nameof(data));

            using (_locker.WriterLock())
            {
                return _writer.Write(_fileMetaBlock, offset, data);
            }
        }

        public void Dispose()
        {
            _locker?.Dispose();
            IsClosed = true;
        }
    }
}