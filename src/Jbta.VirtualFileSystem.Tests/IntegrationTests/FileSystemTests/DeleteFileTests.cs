using System;
using System.Text;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal;
using Jbta.VirtualFileSystem.Internal.Utils;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemTests
{
    public class DeleteFileTests : BaseTests
    {
        private readonly IFileSystem _fileSystem;
        
        public DeleteFileTests()
        {
            _fileSystem = FileSystemManager.Mount(VolumePath);
        }
        
        [Fact]
        public Task DeleteFile_UnmountedFileSystem_FileSystemException()
        {
            // arrange
            FileSystemManager.Unmount(VolumePath);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => _fileSystem.DeleteFile("foo"));
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
            return Assert.ThrowsAsync<ArgumentException>(() => _fileSystem.DeleteFile(fileName));
        }
        
        [Fact]
        public Task DeleteFile_NonExistingFIle_False()
        {
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => _fileSystem.DeleteFile("foobar"));
        }
        
        [Fact]
        public async Task DeleteFile_EmptyFile_FileDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await _fileSystem.CreateFile(fileName);
            
            // act
            var result = await _fileSystem.DeleteFile(fileName);
            
            // assert
            Assert.True(result);
            await Assert.ThrowsAsync<FileSystemException>(() => _fileSystem.OpenFile(fileName));
        }
        
        [Fact]
        public async Task DeleteFile_FileWithData_FileDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await _fileSystem.CreateFile(fileName);
            var file = await _fileSystem.OpenFile(fileName);
            await file.Write(0, Encoding.ASCII.GetBytes(RandomString.Generate(424242)));
            _fileSystem.CloseFile(file);
            
            // act
            var result = await _fileSystem.DeleteFile(fileName);
            
            // assert
            Assert.True(result);
            await Assert.ThrowsAsync<FileSystemException>(() => _fileSystem.OpenFile(fileName));
        }
        
        [Fact]
        public async Task DeleteFile_OpenedFile_CannotBeDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await _fileSystem.CreateFile(fileName);
            await _fileSystem.OpenFile(fileName);
            
            // act
            var result = await _fileSystem.DeleteFile(fileName);
            
            // assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task DeleteFile_ClosedFile_FileDeleted()
        {
            // arrange
            const string fileName = "foobar";
            await _fileSystem.CreateFile(fileName);
            var file = await _fileSystem.OpenFile(fileName);
            _fileSystem.CloseFile(file);
            
            // act
            var result = await _fileSystem.DeleteFile(fileName);
            
            // assert
            Assert.True(result);
            await Assert.ThrowsAsync<FileSystemException>(() => _fileSystem.OpenFile(fileName));
        }
    }
}