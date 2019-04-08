using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks.Serialization;
using Jbta.VirtualFileSystem.Internal.FileOperations;
using Jbta.VirtualFileSystem.Internal.Indexing;
using Jbta.VirtualFileSystem.Internal.Indexing.DataStructure;
using Jbta.VirtualFileSystem.Internal.Mounting;
using Jbta.VirtualFileSystem.Internal.SpaceManagement;

namespace Jbta.VirtualFileSystem.Internal
{
    internal static class FileSystemFactory
    {
        public static IFileSystem New(Volume volume, Superblock superblock, byte[] bitmapBlocks, IndexBlock rootIndexBlock)
        {
            var bitmapTree = new BitmapTree(superblock.BlockSize, bitmapBlocks);
            var bitmap = new Bitmap(bitmapTree, volume);
            var fileSystemMeta = new FileSystemMeta(superblock.BlockSize, bitmap);
            var allocator = new BlocksAllocator(fileSystemMeta, bitmap);
            var deallocator = new BlocksDeallocator(bitmap);
            var superblockSerializer = new SuperblockSerializer(fileSystemMeta.BlockSize);
            var indexBlockSerializer = new IndexBlockSerializer(fileSystemMeta.BlockSize);
            var fileMetaBlockSerializer = new FileMetaBlockSerializer(fileSystemMeta.BlockSize);
            var bPlusNodesFactory = new BPlusTreeNodesPersistenceManager(allocator, deallocator, indexBlockSerializer, volume, volume);
            var index = new FileSystemIndex(bPlusNodesFactory, rootIndexBlock, superblock.RootIndexBlockNumber);
            var fileReader = new FileReader(fileSystemMeta, volume);
            var fileWriter = new FileWriter(fileSystemMeta, allocator, fileMetaBlockSerializer, volume, volume);
            var fileFactory = new FileFactory(fileSystemMeta, fileReader, fileWriter);
            var fileCreator = new FileCreator(index, fileMetaBlockSerializer, allocator, volume);
            var fileOpener = new FileOpener(index, fileFactory, fileMetaBlockSerializer, volume);
            var fileRemover = new FileRemover(index, deallocator, fileMetaBlockSerializer, volume);
            var unmounter = new Unmounter(superblockSerializer, superblock, volume);
            return new FileSystem(volume.VolumePath, fileSystemMeta, fileCreator, fileOpener, fileRemover, unmounter);
        }
    }
}