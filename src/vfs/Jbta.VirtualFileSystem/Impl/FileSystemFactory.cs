using Jbta.VirtualFileSystem.Impl.Blocks;
using Jbta.VirtualFileSystem.Impl.Blocks.Serialization;
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
            var indexBlockSerializer = new IndexBlockSerializer(fileSystemMeta.BlockSize);
            var fileMetaBlockSerializer = new FileMetaBlockSerializer(fileSystemMeta.BlockSize);
            var bPlusNodesFactory = new BPlusTreeNodesFactory(indexBlockSerializer);
            var index = new FileSystemIndex(bPlusNodesFactory, rootIndexBlock);
            var allocator = new Allocator(fileSystemMeta, bitmap);
            var fileReader = new FileReader(fileSystemMeta, volume);
            var fileWriter = new FileWriter(fileSystemMeta, allocator, volume, volume);
            var fileFactory = new FileFactory(fileSystemMeta, fileReader, fileWriter);
            var fileCreator = new FileCreator(index, fileFactory, fileMetaBlockSerializer, allocator, volume);
            var fileOpener = new FileOpener(index, fileFactory, fileMetaBlockSerializer, volume);
            var unmounter = new Unmounter(superblock, volume);
            return new FileSystem(volume.VolumePath, fileSystemMeta, fileCreator, fileOpener, unmounter);
        }
    }
}