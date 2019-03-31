using Jbta.VirtualFileSystem.Mounting;

namespace Jbta.VirtualFileSystem.Impl
{
    internal static class FileSystemFactory
    {
        public static IFileSystem CreateFileSystem(string volumePath, Volume volume, IFileSystemMeta fileSystemMeta)
        {
            var allocator = new Allocator(volume, fileSystemMeta);
            var fileReader = new FileReader(fileSystemMeta, volume);
            var fileWriter = new FileWriter(fileSystemMeta, allocator, volume);
            var fileFactory = new FileFactory(fileReader, fileWriter);
            var fileCreator = new FileCreator(fileFactory, allocator, volume);
            var unmounter = new Unmounter(fileSystemMeta, volume);
            return new FileSystem(volumePath, fileSystemMeta, fileCreator, unmounter);
        }
    }
}