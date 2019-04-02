using System;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.FileSystemManagerTests
{
    public class UnmountTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Unmount_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Unmount(volumePath));
        }
    }
}