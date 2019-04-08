using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal.Utils;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileTests
{
    public class ReadFileTests : BaseTests
    {
        private const string FileName = "foobar";
        private readonly IFileSystem _fileSystem;
        private readonly IFile _file;

        public ReadFileTests()
        {
            _fileSystem = FileSystemManager.Mount(VolumePath);
            _fileSystem.CreateFile(FileName).Wait();
            _file = _fileSystem.OpenFile(FileName).Result;
        }
        
        [Fact]
        public Task ReadFile_ClosedFile_FileSystemException()
        {
            // arrange
            _fileSystem.CloseFile(_file);
            
            // act, assert
            return Assert.ThrowsAsync<FileSystemException>(() => _file.Read(0, 42));
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(-42)]
        public Task ReadFile_InvalidOffset_ArgumentOutOfRangeException(int offset)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _file.Read(offset, 42));
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-42)]
        public Task ReadFile_InvalidLength_ArgumentOutOfRangeException(int length)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _file.Read(0, length));
        }

        [Fact]
        public async Task Read_MultipleThreads_SuccessfulRead()
        {
            // arrange
            const int size = 42;
            var content = RandomString.Generate(size);
            await _file.Write(0, Encoding.ASCII.GetBytes(content));
            
            // act
            var readContents = new List<string>();
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() =>
                {
                    var readContent = _file.Read(0, size).Result;
                    readContents.Add(Encoding.ASCII.GetString(readContent.ToArray()));
                }))
                .ToList();
            await Task.WhenAll(tasks);
            
            // assert
            Assert.All(readContents, c => Assert.Equal(content, c));
        }
    }
}