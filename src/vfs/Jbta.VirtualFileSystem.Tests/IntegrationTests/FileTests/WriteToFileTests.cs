using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal;
using Jbta.VirtualFileSystem.Internal.Utils;
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
        public async Task Write_LessThanBlock_ExpectedContent()
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
            Assert.Equal(2 * GlobalConstant.DefaultBlockSize, await _file.Size);
        }
        
        [Fact]
        public async Task Write_ExactlyOneBlock_ExpectedContent()
        {
            // arrange
            var originalContent = RandomString.Generate(GlobalConstant.DefaultBlockSize);
            var bytes = Encoding.ASCII.GetBytes(originalContent);
            
            // act
            await _file.Write(0, bytes);
            
            // assert
            var data = (await _file.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(2 * GlobalConstant.DefaultBlockSize, await _file.Size);
        }
        
        [Fact]
        public async Task Write_AllDirectBlocks_ExpectedContent()
        {
            // arrange
            const int blocksCount = GlobalConstant.MaxFileDirectBlocksCount;
            const int size = GlobalConstant.DefaultBlockSize * blocksCount;
            var originalContent = RandomString.Generate(size);
            var bytes = Encoding.ASCII.GetBytes(originalContent);
            
            // act
            await _file.Write(0, bytes);
            
            // assert
            var data = (await _file.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(size + GlobalConstant.DefaultBlockSize, await _file.Size);
        }
        
        [Fact]
        public async Task Write_DirectAndOneIndirectBlocks_ExpectedContent()
        {
            // arrange
            const int dataBlocksCount = GlobalConstant.MaxFileDirectBlocksCount + 42;
            const int size = GlobalConstant.DefaultBlockSize * dataBlocksCount;
            var originalContent = RandomString.Generate(size);
            var bytes = Encoding.ASCII.GetBytes(originalContent);
            
            // act
            await _file.Write(0, bytes);
            
            // assert
            var data = (await _file.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(size + GlobalConstant.DefaultBlockSize, await _file.Size);
        }
        
        [Fact]
        public async Task Write_MaxSizeFile_ExpectedContent()
        {
            // arrange
            const int dataBlocksCount =
                GlobalConstant.MaxFileDirectBlocksCount
                + GlobalConstant.MaxFileIndirectBlocksCount * GlobalConstant.DefaultBlockSize / sizeof(int);
            const int size = GlobalConstant.DefaultBlockSize * dataBlocksCount;
            var originalContent = RandomString.Generate(size);
            var bytes = Encoding.ASCII.GetBytes(originalContent);
            
            // act
            await _file.Write(0, bytes);
            
            // assert
            var data = (await _file.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(size + GlobalConstant.DefaultBlockSize, await _file.Size);
        }
        
        [Fact]
        public Task Write_MaxSizeFileExceeded_FileSystemException()
        {
            // arrange
            const int dataBlocksCount =
                GlobalConstant.MaxFileDirectBlocksCount
                + GlobalConstant.MaxFileIndirectBlocksCount * GlobalConstant.DefaultBlockSize / sizeof(int) + 1;
            const int size = GlobalConstant.DefaultBlockSize * dataBlocksCount;
            var originalContent = RandomString.Generate(size);
            var bytes = Encoding.ASCII.GetBytes(originalContent);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => _file.Write(0, bytes));
        }
        
        [Fact]
        public async Task Write_AppendingInFile_ExpectedContent()
        {
            // arrange
            const int length = 4242;
            var originalContent1 = RandomString.Generate(length);
            var originalContent2 = RandomString.Generate(length);
            var bytes1 = Encoding.ASCII.GetBytes(originalContent1);
            var bytes2 = Encoding.ASCII.GetBytes(originalContent2);
            await _file.Write(0, bytes1);
            
            // act
            await _file.Write(length, bytes2);
            
            // assert
            var data = (await _file.Read(0, 2 * length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(2 * length, data.Length);
            Assert.Equal(originalContent1 + originalContent2, Encoding.ASCII.GetString(data));
            Assert.Equal(2 * length * GlobalConstant.DefaultBlockSize, await _file.Size);
        }
    }
}