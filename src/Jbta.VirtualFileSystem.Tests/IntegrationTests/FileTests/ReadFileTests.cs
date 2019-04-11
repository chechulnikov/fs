using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.Utils;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileTests
{
    public class ReadFileTests : TestsWithFileBase
    {
        [Fact]
        public Task ReadFile_ClosedFile_FileSystemException()
        {
            // arrange
            FileSystem.CloseFile(File);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => File.Read(0, 42));
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(-42)]
        public Task ReadFile_InvalidOffset_ArgumentOutOfRangeException(int offset)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => File.Read(offset, 42));
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-42)]
        public Task ReadFile_InvalidLength_ArgumentOutOfRangeException(int length)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => File.Read(0, length));
        }

        [Fact]
        public async Task Read_MultipleThreads_SuccessfulRead()
        {
            // arrange
            const int size = 42;
            var content = RandomString.Generate(size);
            await File.Write(0, Encoding.ASCII.GetBytes(content));
            
            // act
            var readContents = new List<string>();
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(async () =>
                {
                    var readContent = await File.Read(0, size);
                    readContents.Add(Encoding.ASCII.GetString(readContent.ToArray()));
                }))
                .ToList();
            await Task.WhenAll(tasks);
            
            // assert
            Assert.All(readContents, c => Assert.Equal(content, c));
        }
    }
}