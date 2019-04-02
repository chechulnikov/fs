using Xunit;

namespace Jbta.VirtualFileSystem.Tests.FileSystemManagerTests
{
    public class MountedFileSystemsTests
    {
        [Fact]
        public void MountedFileSystem_HappyPath_NotNull()
        {
            Assert.NotNull(FileSystemManager.MountedFileSystems);
        }
    }
}