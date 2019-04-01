using System;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests
{
    public class FileSystemManagerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public Task Init_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            // Arrange
            var settings = new FileSystemSettings {VolumePath = volumePath};
            
            // Assert
            return Assert.ThrowsAsync<FileSystemInitException>(() => FileSystemManager.Init(settings));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Mount_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Mount(volumePath));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Unmount_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Unmount(volumePath));
        }
        
        [Fact]
        public void MountedFileSystem_HappyPath_NotNull()
        {
            Assert.NotNull(FileSystemManager.MountedFileSystems);
        }
    }
}