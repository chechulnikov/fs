using System;

namespace Vfs
{
    internal class Allocator
    {
        private readonly Volume _volume;
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly Bitmap _bitmap;

        public Allocator(Volume volume, IFileSystemMeta fileSystemMeta)
        {
            _volume = volume;
            _fileSystemMeta = fileSystemMeta;
            _bitmap = new Bitmap();
        }

        public ReadOnlyMemory<int> Allocate(int countOfBlocks)
        {
            throw new NotImplementedException();
        }
    }
}