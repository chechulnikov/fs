using System;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemTests
{
    public class CloseFileTests : TestsWithMountBase
    {
        [Fact]
        public void CloseFile_UnmountedFileSystem_FileSystemException()
        {
            // arrange
            FileSystemManager.Unmount(VolumePath);
            
            // act, assert
            Assert.Throws<FileSystemException>(() => FileSystem.CloseFile(null));
        }
        
        [Fact]
        public void CloseFile_FileIsNull_ArgumentNullException()
        {
            // act, assert
            Assert.Throws<ArgumentNullException>(() => FileSystem.CloseFile(null));
        }

        [Fact]
        public async Task CloseFile_HappyPath_FileClosed()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            var file = await FileSystem.OpenFile(fileName);
            
            // act
            var result = FileSystem.CloseFile(file);
            
            // assert
            Assert.True(result);
            Assert.True(file.IsClosed);
        }

        [Fact]
        public async Task CloseFile_NotOpenedFile_False()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            var file = await FileSystem.OpenFile(fileName);
            FileSystem.CloseFile(file);
            
            // act
            var result = FileSystem.CloseFile(file);
            
            // assert
            Assert.False(result);
        }
    }
}