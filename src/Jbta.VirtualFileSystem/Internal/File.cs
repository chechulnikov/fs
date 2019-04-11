using System;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.FileOperations;
using Nito.AsyncEx;

namespace Jbta.VirtualFileSystem.Internal
{
    internal class File : IFile
    {
        private readonly FileReader _reader;
        private readonly FileWriter _writer;
        private readonly FileSizeMeter _sizeMeter;
        private readonly FileMetaBlock _fileMetaBlock;
        private readonly AsyncReaderWriterLock _locker;

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
            _locker = new AsyncReaderWriterLock();
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

        public async Task<Memory<byte>> Read(int offset, int length)
        {
            if (IsClosed) throw new FileSystemException("Cannot read from closed file");
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            using (await _locker.ReaderLockAsync())
            {
                return await _reader.Read(_fileMetaBlock, offset, length);
            }
        }

        public async Task Write(int offset, byte[] data)
        {
            if (IsClosed) throw new FileSystemException("Cannot write to closed file");
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (data == null || data.Length == 0) throw new ArgumentException(nameof(data));

            using (await _locker.WriterLockAsync())
            {
                await _writer.Write(_fileMetaBlock, offset, data);
            }
        }

        public void Dispose()
        {
            IsClosed = true;
        }
    }
}