using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal;
using Jbta.VirtualFileSystem.Internal.Utils;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileTests
{
    public class WriteToFileTests : TestsWithFileBase
    {
        [Fact]
        public Task Write_ClosedFile_FileSystemException()
        {
            // arrange
            FileSystem.CloseFile(File);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => File.Write(0, null));
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(-42)]
        public Task Write_InvalidOffset_ArgumentOutOfRangeException(int offset)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => File.Write(offset, null));
        }
        
        [Fact]
        public Task Write_InvalidData_ArgumentException()
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => File.Write(0, null));
        }
        
        [Fact]
        public Task Write_EmptyData_ArgumentException()
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => File.Write(0, new byte[]{}));
        }

        [Fact]
        public async Task Write_LessThanBlock_ExpectedContent()
        {
            // arrange
            const string originalContent = "some content";
            
            // act
            await File.Write(0, Encoding.ASCII.GetBytes(originalContent));
            
            // assert
            var data = (await File.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            var content = Encoding.ASCII.GetString(data);
            Assert.Equal(originalContent, content);
            Assert.Equal(2 * GlobalConstant.DefaultBlockSize, await File.Size);
        }
        
        [Fact]
        public async Task Write_ExactlyOneBlock_ExpectedContent()
        {
            // arrange
            var originalContent = RandomString.Generate(GlobalConstant.DefaultBlockSize);
            var bytes = Encoding.ASCII.GetBytes(originalContent);
            
            // act
            await File.Write(0, bytes);
            
            // assert
            var data = (await File.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(2 * GlobalConstant.DefaultBlockSize, await File.Size);
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
            await File.Write(0, bytes);
            
            // assert
            var data = (await File.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(size + GlobalConstant.DefaultBlockSize, await File.Size);
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
            await File.Write(0, bytes);
            
            // assert
            var data = (await File.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(size + GlobalConstant.DefaultBlockSize, await File.Size);
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
            await File.Write(0, bytes);
            
            // assert
            var data = (await File.Read(0, originalContent.Length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(originalContent.Length, data.Length);
            Assert.Equal(originalContent, Encoding.ASCII.GetString(data));
            Assert.Equal(size + GlobalConstant.DefaultBlockSize, await File.Size);
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
            return Assert.ThrowsAsync<FileSystemException>(() => File.Write(0, bytes));
        }
        
        [Theory]
        [InlineData(42, 2 * GlobalConstant.DefaultBlockSize)]
        [InlineData(1024, 3 * GlobalConstant.DefaultBlockSize)]
        [InlineData(1500, 4 * GlobalConstant.DefaultBlockSize)]
        [InlineData(GlobalConstant.DefaultBlockSize * 16 + 2, (16 * 2 + 2) * GlobalConstant.DefaultBlockSize)]
        [InlineData(GlobalConstant.DefaultBlockSize * 24 + 2, (24 * 2 + 2) * GlobalConstant.DefaultBlockSize)]
        public async Task Write_AppendingInFile_ExpectedContent(int length, int expectedSize)
        {
            // arrange
            var originalContent1 = RandomString.Generate(length);
            var originalContent2 = RandomString.Generate(length);
            var bytes1 = Encoding.ASCII.GetBytes(originalContent1);
            var bytes2 = Encoding.ASCII.GetBytes(originalContent2);
            await File.Write(0, bytes1);
            
            // act
            await File.Write(length, bytes2);
            
            // assert
            var data = (await File.Read(0, 2 * length)).ToArray();
            Assert.NotNull(data);
            Assert.Equal(2 * length, data.Length);
            Assert.Equal(originalContent1 + originalContent2, Encoding.ASCII.GetString(data));
            Assert.Equal(expectedSize, await File.Size);
        }
        
        [Fact]
        public async Task Write_SeveralFilesSimultaneously_ExpectedContent()
        {
            // arrange
            const int dataBlocksCount = GlobalConstant.MaxFileDirectBlocksCount + 42;
            const int size = GlobalConstant.DefaultBlockSize * dataBlocksCount;
            var originalContent = RandomString.Generate(size);
            var bytes = Encoding.ASCII.GetBytes(originalContent);
            var fileNames = new List<string>();
            
            // act
            await Task.WhenAll(
                Enumerable.Range(0, 20)
                    .Select(_ => Task.Run(async () =>
                    {
                        var fileName = RandomString.Generate(16);
                        fileNames.Add(fileName);
                        await FileSystem.CreateFile(fileName);
                        var file = await FileSystem.OpenFile(fileName);
                        await file.Write(0, bytes);
                    }))
                    .ToList()
            );
            
            // assert
            var readContents = new List<string>();
            foreach (var fileName in fileNames)
            {
                var file = await FileSystem.OpenFile(fileName);
                var readContent = await file.Read(0, size);
                readContents.Add(Encoding.ASCII.GetString(readContent.ToArray()));
            }
            Assert.All(readContents, c => Assert.Equal(originalContent, c));
        }
    }
}