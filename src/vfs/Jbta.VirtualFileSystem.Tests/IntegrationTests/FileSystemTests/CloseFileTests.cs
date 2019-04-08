using System;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemTests
{
    public class CloseFileTests : BaseTests
    {
        private readonly IFileSystem _fileSystem;
        
        public CloseFileTests()
        {
            _fileSystem = FileSystemManager.Mount(VolumePath);
        }

        [Fact]
        public void CloseFile_UnmountedFileSystem_FileSystemException()
        {
            // arrange
            FileSystemManager.Unmount(VolumePath);
            
            // act, assert
            Assert.Throws<FileSystemException>(() => _fileSystem.CloseFile(null));
        }
        
        [Fact]
        public void CloseFile_FileIsNull_ArgumentNullException()
        {
            // act, assert
            Assert.Throws<ArgumentNullException>(() => _fileSystem.CloseFile(null));
        }

        [Fact]
        public async Task CloseFile_HappyPath_FileClosed()
        {
            // arrange
            const string fileName = "foobar";
            await _fileSystem.CreateFile(fileName);
            var file = await _fileSystem.OpenFile(fileName);
            
            // act
            var result = _fileSystem.CloseFile(file);
            
            // assert
            Assert.True(result);
            Assert.True(file.IsClosed);
        }

        [Fact]
        public async Task CloseFile_NotOpenedFile_False()
        {
            // arrange
            const string fileName = "foobar";
            await _fileSystem.CreateFile(fileName);
            var file = await _fileSystem.OpenFile(fileName);
            _fileSystem.CloseFile(file);
            
            // act
            var result = _fileSystem.CloseFile(file);
            
            // assert
            Assert.False(result);
        }
    }
}