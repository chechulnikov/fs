namespace Vfs
{
    internal static class FileSystemFactory
    {
        public static IFileSystem CreateFileSystem(string volumePath, Volume volume, IFileSystemMeta fileSystemMeta)
        {
            var transactionManager = new TransactionManager();
            var allocator = new Allocator(volume, fileSystemMeta);
            var fileReader = new FileReader(fileSystemMeta, transactionManager, volume);
            var fileFactory = new FileFactory(fileReader);
            var fileCreator = new FileCreator(fileFactory, transactionManager, allocator);
            return new FileSystem(volumePath, fileSystemMeta, fileCreator);
        }
    }
}