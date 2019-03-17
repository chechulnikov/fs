using System;
using Xunit;

namespace Vfs.Tests
{
    public class FileSystemManagerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Init_DeviceFileNameIsNull_ArgumentException(string deviceFileName)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Init(deviceFileName));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Mount_DeviceFileNameIsNull_ArgumentException(string deviceFileName)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Mount(deviceFileName));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Unmount_DeviceFileNameIsNull_ArgumentException(string deviceFileName)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Unmount(deviceFileName));
        }
        
        [Fact]
        public void MountedFileSystem_HappyPath_NotNull()
        {
            Assert.NotNull(FileSystemManager.MountedFileSystems);
        }
    }
}