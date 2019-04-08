using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests.FileSystemManagerTests
{
    public class InitTests : IDisposable
    {
        private readonly string _volumePath;
        
        public InitTests()
        {
            _volumePath = $"test-volume_{Guid.NewGuid()}.dat";
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public Task Init_VolumePathIsInvalid_ArgumentException(string volumePath)
        {
            return Assert.ThrowsAsync<ArgumentException>(() => FileSystemManager.Init(volumePath));
        }

        [Fact]
        public async Task Init_HappyPath_FileWasCreatedAndHasExpectedSize()
        {
            // act
            await FileSystemManager.Init(_volumePath);
            
            // assert
            Assert.True(System.IO.File.Exists(_volumePath));
            Assert.Equal(
                GlobalConstant.DefaultBlockSize * (GlobalConstant.BitmapBlocksCount + 2),
                new FileInfo(_volumePath).Length
            );
        }
        
        [Fact]
        public async Task Init_HappyPath_ValidSuperblock()
        {
            // act
            await FileSystemManager.Init(_volumePath);
            
            // assert
            var superblockBytes = new byte[GlobalConstant.DefaultBlockSize];
            using (var fs = System.IO.File.OpenRead(_volumePath)) await fs.ReadAsync(superblockBytes);
            var offset = 0;
            Assert.Equal(GlobalConstant.SuperblockMagicNumber, BitConverter.ToInt32(superblockBytes));
            Assert.False(BitConverter.ToBoolean(superblockBytes, offset += sizeof(int)));
            Assert.Equal(GlobalConstant.DefaultBlockSize, BitConverter.ToInt32(superblockBytes, offset += sizeof(bool)));
            Assert.Equal(
                GlobalConstant.BitmapBlocksCount + 1,
                BitConverter.ToInt32(superblockBytes, offset + sizeof(int))
            );
        }
        
        [Fact]
        public async Task Init_HappyPath_ValidBitmap()
        {
            // act
            await FileSystemManager.Init(_volumePath);
            
            // assert
            var bitmapBytes = new byte[GlobalConstant.DefaultBlockSize * GlobalConstant.BitmapBlocksCount];
            using (var fs = System.IO.File.OpenRead(_volumePath))
            {
                fs.Seek(GlobalConstant.DefaultBlockSize, SeekOrigin.Begin);
                await fs.ReadAsync(bitmapBytes);
            }
            var bitmap = new BitArray(bitmapBytes);
            for (var i = 0; i < GlobalConstant.BitmapBlocksCount + 2; i++)
            {
                Assert.True(bitmap[i]);
            }
            for (var i = GlobalConstant.BitmapBlocksCount + 2; i < bitmap.Count; i++)
            {
                Assert.False(bitmap[i]);
            }
        }

        public void Dispose()
        {
            if (System.IO.File.Exists(_volumePath)) System.IO.File.Delete(_volumePath);
        }
    }
}