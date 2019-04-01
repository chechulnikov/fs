using Jbta.VirtualFileSystem.Impl.Indexing;
using Jbta.VirtualFileSystem.Mounting;

namespace Jbta.VirtualFileSystem.Impl
{
    internal static class FileSystemFactory
    {
        public static IFileSystem New(Volume volume, Superblock superblock, byte[] bitmapBlocks, byte[] rootIndexBlock)
        {
            var bitmap = new BitmapTree(bitmapBlocks);
            var fileSystemMeta = new FileSystemMeta(superblock.BlockSize, bitmap);
            var index = new FileSystemIndex(fileSystemMeta, rootIndexBlock);
            var allocator = new Allocator(fileSystemMeta, bitmap);
            var fileReader = new FileReader(fileSystemMeta, volume);
            var fileWriter = new FileWriter(fileSystemMeta, allocator, volume, volume);
            var fileFactory = new FileFactory(fileSystemMeta, fileReader, fileWriter);
            var fileCreator = new FileCreator(index, fileFactory, allocator, volume);
            var unmounter = new Unmounter(superblock, volume);
            var fileOpener = new FileOpener(index, fileFactory, volume);
            return new FileSystem(volume.VolumePath, fileSystemMeta, fileCreator, fileOpener, unmounter);
        }
    }
}