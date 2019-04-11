using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemManagerTests
{
    public class UnmountTests : TestsWithInitBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Unmount_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Unmount(volumePath));
        }
        
        [Fact]
        public async Task Unmount_MultithreadedEnv_UnmountedOnlyOnce()
        {
            // arrange
            await FileSystemManager.Mount(VolumePath);
            
            // act
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() => FileSystemManager.Unmount(VolumePath)))
                .ToList();
            await Task.WhenAll(tasks);

            // assert
            Assert.False(FileSystemManager.MountedFileSystems.ContainsKey(VolumePath));
        }
    }
}