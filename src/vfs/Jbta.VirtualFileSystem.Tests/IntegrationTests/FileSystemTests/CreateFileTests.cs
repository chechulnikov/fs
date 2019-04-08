using System;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemTests
{
    public class CreateFileTests : BaseTests
    {
        private readonly IFileSystem _fileSystem;
        
        public CreateFileTests()
        {
            _fileSystem = FileSystemManager.Mount(VolumePath);
        }
        
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
        public Task CreateFile_TooLargeFileName_ArgumentException(string fileName)
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
        
        [Fact]
        public async Task CreateFile_ManyFiles_OpensSuccessful()
        {
            // act
            const int quantity = 100;
            foreach (var i in Enumerable.Range(1, quantity))
            {
                await _fileSystem.CreateFile($"foo{i}");
            }
            
            // assert
            foreach (var i in Enumerable.Range(1, quantity))
            {
                var fileName = $"foo{i}";
                var file = await _fileSystem.OpenFile(fileName);
                Assert.NotNull(file);
                Assert.Equal(fileName, file.Name);
                Assert.Equal(GlobalConstant.DefaultBlockSize, file.Size);
            }
        }
    }
}