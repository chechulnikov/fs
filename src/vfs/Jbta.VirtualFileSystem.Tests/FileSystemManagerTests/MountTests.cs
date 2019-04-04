using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.FileSystemManagerTests
{
    public class MountTests : BaseTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        [InlineData("foobar")]
        public void Mount_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            // act, assert
            Assert.Throws<ArgumentException>(() => FileSystemManager.Mount(volumePath));
        }
        
        [Fact]
        public void Mount_HappyPath_ValidFileSystem()
        {
            // act
            var fileSystem = FileSystemManager.Mount(VolumePath);

            // assert
            Assert.NotNull(fileSystem);
            Assert.True(fileSystem.IsMounted);
            Assert.Equal(VolumePath, fileSystem.VolumePath);
            Assert.NotEqual(0ul, fileSystem.VolumeSize);
            Assert.NotEqual(0ul, fileSystem.UsedSpace);
            Assert.NotEqual(0ul, fileSystem.UnusedSpace);
        }

        [Fact]
        public void Mount_FileSystemHasAlreadyMounted_SameObject()
        {
            // arrange
            var first = FileSystemManager.Mount(VolumePath);

            // act
            var second = FileSystemManager.Mount(VolumePath);

            // assert
            Assert.Same(first, second);
        }
        
        [Fact]
        public async Task Mount_MultithreadedEnv_SameObject()
        {
            // act
            var fileSystems = new List<IFileSystem>();
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() =>
                {
                    var fileSystem = FileSystemManager.Mount(VolumePath);
                    fileSystems.Add(fileSystem);
                }))
                .ToList();
            await Task.WhenAll(tasks);

            // assert
            var anyFs = fileSystems.First();
            Assert.All(fileSystems, fs => Assert.Same(anyFs, fs));
        }
    }
}