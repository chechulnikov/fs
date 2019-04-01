using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Xunit;

namespace Vfs.Tests
{
    public class InitializerTest : IDisposable
    {
        private readonly string _volumePath;
        private readonly Initializer _initializer;
        
        public InitializerTest()
        {
            _volumePath = $"test_volume_{Guid.NewGuid()}.dat";
            _initializer = new Initializer();
        }

        [Fact]
        public async Task Initialize_HappyPath_FileExistsAndReadable()
        {
            // Arrange
            var settings = new FileSystemSettings { VolumePath = _volumePath };
            
            // Act
            await _initializer.Initialize(settings);
            
            // Assert
            Assert.True(File.Exists(_volumePath));
            var superblock = await ReadSuperblock(settings.BlockSize);
            Assert.Equal(settings.BlockSize, superblock.BlockSize);
        }

        private async Task<Superblock> ReadSuperblock(int blockSize)
        {
            var bytes = new byte[blockSize];
            
            using (var fs = new FileStream(_volumePath, FileMode.Open))
                await fs.ReadAsync(bytes, 0,blockSize);

            using (var ms = new MemoryStream(bytes))
                return (Superblock) new BinaryFormatter().Deserialize(ms);
        }
        
        public void Dispose()
        {
            if (File.Exists(_volumePath)) File.Delete(_volumePath);
        }
    }
}