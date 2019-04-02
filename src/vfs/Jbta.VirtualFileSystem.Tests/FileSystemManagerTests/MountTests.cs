using System;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.FileSystemManagerTests
{
    public class MountTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Mount_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            Assert.Throws<ArgumentException>(() => FileSystemManager.Mount(volumePath));
        }
    }
}