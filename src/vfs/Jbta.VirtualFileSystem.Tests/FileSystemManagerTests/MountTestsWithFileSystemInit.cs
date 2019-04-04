using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.FileSystemManagerTests
{
    public class MountTestsWithFileSystemInit : BaseTestsWithFileSystemInit
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        [InlineData("foobar")]
        public Task Mount_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            return Assert.ThrowsAsync<ArgumentException>(() => FileSystemManager.Mount(volumePath));
        }
        
        [Fact]
        public async Task Mount_HappyPath_ValidFileSystem()
        {
            // act
            var fileSystem = await FileSystemManager.Mount(VolumePath);

            // assert
            Assert.NotNull(fileSystem);
            Assert.True(fileSystem.IsMounted);
            Assert.Equal(VolumePath, fileSystem.VolumePath);
            Assert.NotEqual(0ul, fileSystem.VolumeSize);
            Assert.NotEqual(0ul, fileSystem.UsedSpace);
            Assert.NotEqual(0ul, fileSystem.UnusedSpace);
        }

        [Fact]
        public async Task Mount_FileSystemHasAlreadyMounted_SameObject()
        {
            // arrange
            var first = await FileSystemManager.Mount(VolumePath);

            // act
            var second = await FileSystemManager.Mount(VolumePath);

            // assert
            Assert.Same(first, second);
        }
        
        [Fact]
        public async Task Mount_MultithreadedEnv_SameObject()
        {
            // arrange
            var first = await FileSystemManager.Mount(VolumePath);

            // act
            var fileSystems = new List<IFileSystem>();
            new Thread(() =>
            {
                var second = await FileSystemManager.Mount(VolumePath);
            }).Start();

            // assert
            Assert.Same(first, second);
        }
    }
}