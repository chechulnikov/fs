using System;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemTests
{
    public class OpenFileTests : TestsWithMountBase
    {
        [Fact]
        public Task OpenFile_UnmountedFileSystem_FileSystemException()
        {
            // arrange
            FileSystemManager.Unmount(VolumePath);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => FileSystem.OpenFile("foo"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public Task OpenFile_InvalidFileName_ArgumentException(string fileName)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => FileSystem.OpenFile(fileName));
        }
        
        [Theory]
        [InlineData("12345678901234567")]
        [InlineData(" 12345678901234567   ")]
        public Task OpenFile_TooLargeFileName_ArgumentException(string fileName)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => FileSystem.CreateFile(fileName));
        }

        [Fact]
        public async Task OpenFile_HappyPath_ValidFile()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            
            // act
            var file = await FileSystem.OpenFile(fileName);
            
            // assert
            Assert.NotNull(file);
            Assert.Equal(fileName, file.Name);
            Assert.Equal(GlobalConstant.DefaultBlockSize, await file.Size);
        }
        
        [Fact]
        public async Task OpenFile_AlreadyOpened_SameObject()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            var file1 = await FileSystem.OpenFile(fileName);
            
            // act
            var file2 = await FileSystem.OpenFile(fileName);
            
            // assert
            Assert.Same(file1, file2);
        }
        
        [Fact]
        public async Task OpenFile_ClosedFile_AnotherObject()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            var file1 = await FileSystem.OpenFile(fileName);
            FileSystem.CloseFile(file1);
            
            // act
            var file2 = await FileSystem.OpenFile(fileName);
            
            // assert
            Assert.NotSame(file1, file2);
        }
    }
}