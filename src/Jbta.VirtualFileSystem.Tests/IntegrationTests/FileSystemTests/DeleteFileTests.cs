using System;
using System.Text;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.Utils;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemTests
{
    public class DeleteFileTests : TestsWithMountBase
    {
        [Fact]
        public Task DeleteFile_UnmountedFileSystem_FileSystemException()
        {
            // arrange
            FileSystemManager.Unmount(VolumePath);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => FileSystem.DeleteFile("foo"));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        [InlineData("12345678901234567")]
        [InlineData(" 12345678901234567   ")]
        public Task DeleteFile_InvalidFileName_ArgumentException(string fileName)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => FileSystem.DeleteFile(fileName));
        }
        
        [Fact]
        public Task DeleteFile_NonExistingFIle_False()
        {
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => FileSystem.DeleteFile("foobar"));
        }
        
        [Fact]
        public async Task DeleteFile_EmptyFile_FileDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            
            // act
            var result = await FileSystem.DeleteFile(fileName);
            
            // assert
            Assert.True(result);
            await Assert.ThrowsAsync<FileSystemException>(() => FileSystem.OpenFile(fileName));
        }
        
        [Fact]
        public async Task DeleteFile_FileWithData_FileDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            var file = await FileSystem.OpenFile(fileName);
            await file.Write(0, Encoding.ASCII.GetBytes(RandomString.Generate(424242)));
            FileSystem.CloseFile(file);
            
            // act
            var result = await FileSystem.DeleteFile(fileName);
            
            // assert
            Assert.True(result);
            await Assert.ThrowsAsync<FileSystemException>(() => FileSystem.OpenFile(fileName));
        }
        
        [Fact]
        public async Task DeleteFile_OpenedFile_CannotBeDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            await FileSystem.OpenFile(fileName);
            
            // act
            var result = await FileSystem.DeleteFile(fileName);
            
            // assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task DeleteFile_ClosedFile_FileDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await FileSystem.CreateFile(fileName);
            var file = await FileSystem.OpenFile(fileName);
            FileSystem.CloseFile(file);
            
            // act
            var result = await FileSystem.DeleteFile(fileName);
            
            // assert
            Assert.True(result);
            await Assert.ThrowsAsync<FileSystemException>(() => FileSystem.OpenFile(fileName));
        }
    }
}