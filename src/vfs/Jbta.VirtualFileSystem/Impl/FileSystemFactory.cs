using Jbta.VirtualFileSystem.Impl.Indexing;
using Jbta.VirtualFileSystem.Mounting;

namespace Jbta.VirtualFileSystem.Impl
{
    internal static class FileSystemFactory
    {
        public static IFileSystem New(Volume volume, Superblock superblock)
        {
            var bitmap = new BitmapTree(ReadBitmapBlocks(volume));
            var fileSystemMeta = new FileSystemMeta(superblock.BlockSize, bitmap);
            var allocator = new Allocator(fileSystemMeta, bitmap);
            var fileReader = new FileReader(fileSystemMeta, volume);
            var fileWriter = new FileWriter(fileSystemMeta, allocator, volume);
            var fileFactory = new FileFactory(fileReader, fileWriter);
            var fileCreator = new FileCreator(fileFactory, allocator, volume);
            var unmounter = new Unmounter(superblock, volume);
            var index = new FileSystemIndex(fileSystemMeta); // todo
            var fileOpener = new FileOpener(index);
            return new FileSystem(volume.VolumePath, fileSystemMeta, fileCreator, fileOpener, unmounter);
        }
        
        private static byte[] ReadBitmapBlocks(Volume volume)
        {
            return volume.ReadBlocks(1, GlobalConstant.BitmapBlocksCount).Result;
        }
    }
}