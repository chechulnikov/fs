using System;
using System.Text;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileTests
{
    public class WriteToFileTests : BaseTests
    {
        private const string FileName = "foobar";
        private readonly IFileSystem _fileSystem;
        private readonly IFile _file;

        public WriteToFileTests()
        {
            _fileSystem = FileSystemManager.Mount(VolumePath);
            _fileSystem.CreateFile(FileName).Wait();
            _file = _fileSystem.OpenFile(FileName).Result;
        }
        
        [Fact]
        public Task Write_ClosedFile_FileSystemException()
        {
            // arrange
            _fileSystem.CloseFile(_file);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => _file.Write(0, null));
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(-42)]
        public Task Write_InvalidOffset_ArgumentOutOfRangeException(int offset)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _file.Write(offset, null));
        }
        
        [Fact]
        public Task Write_InvalidData_ArgumentException()
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => _file.Write(0, null));
        }
        
        [Fact]
        public Task Write_EmptyData_ArgumentException()
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => _file.Write(0, new byte[]{}));
        }

        [Fact]
        public async Task Write_HappyPath_ExpectedContent()
        {
            // arrange
            const string originalContent = "some content";
            
            // act
            await _file.Write(0, Encoding.ASCII.GetBytes(originalContent));
            
            // assert
            var data = (await _file.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            var content = Encoding.ASCII.GetString(data);
            Assert.Equal(originalContent, content);
        }
    }
}