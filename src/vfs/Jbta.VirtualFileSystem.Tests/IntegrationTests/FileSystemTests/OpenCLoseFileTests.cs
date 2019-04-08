using System;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemTests
{
    public class OpenCloseFileTests : BaseTests
    {
        private readonly IFileSystem _fileSystem;
        
        public OpenCloseFileTests()
        {
            _fileSystem = FileSystemManager.Mount(VolumePath);
        }
        // TODO !!!!!!!!
        [Fact]
        public Task CreateFile_UnmountedFileSystem_FileSystemException()
        {
            // arrange
            FileSystemManager.Unmount(VolumePath);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => _fileSystem.CreateFile("foo"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public Task CreateFile_InvalidFileName_ArgumentException(string fileName)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => _fileSystem.CreateFile(fileName));
        }
        
        [Theory]
        [InlineData("12345678901234567")]
        [InlineData(" 12345678901234567   ")]
        public Task CreateFile_TooLargeFileName_FileSystemException(string fileName)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => _fileSystem.CreateFile(fileName));
        }

        [Fact]
        public async Task CreateFile_HappyPath_ValidFile()
        {
            // act
            const string fileName = "foobar";
            await _fileSystem.CreateFile(fileName);
            
            // assert
            var file = await _fileSystem.OpenFile(fileName);
            Assert.NotNull(file);
            Assert.Equal(fileName, file.Name);
            Assert.Equal(GlobalConstant.DefaultBlockSize, file.Size);
        }
    }
}