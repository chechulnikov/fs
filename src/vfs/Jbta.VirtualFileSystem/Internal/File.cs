using System;
using System.Threading;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.Blocks;
using Jbta.VirtualFileSystem.Internal.FileOperations;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal
{
    internal class File : IFile
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly FileReader _reader;
        private readonly FileWriter _writer;
        private readonly FileMetaBlock _fileMetaBlock;
        private readonly ReaderWriterLockSlim _locker;

        public File(FileSystemMeta fileSystemMeta, FileReader reader, FileWriter writer, FileMetaBlock fileMetaBlock, string name)
        {
            _fileSystemMeta = fileSystemMeta;
            _reader = reader;
            _writer = writer;
            _fileMetaBlock = fileMetaBlock;
            Name = name;
            _locker = new ReaderWriterLockSlim();
        }

        public string Name { get; }

        public int Size => CalcDataBlocksSizeInBytes(_fileSystemMeta.BlockSize) + _fileSystemMeta.BlockSize;

        public Task<Memory<byte>> Read(int offset, int length)
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

        public void Dispose() => _locker?.Dispose();
        
        private int CalcDataBlocksSizeInBytes(int blockSize)
        {
            var indirectBlockCapacity = blockSize / sizeof(int);
            return (_fileMetaBlock.DirectBlocksCount + _fileMetaBlock.IndirectBlocksCount * indirectBlockCapacity) * blockSize;
        }
    }
}