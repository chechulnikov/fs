using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Vfs.Mounting
{
    internal class Mounter
    {
        public async ValueTask<IFileSystem> Mount(string volumePath)
        {
            if (!File.Exists(volumePath)) throw new VolumeNotFoundException(volumePath);
            var blockSize = ValidateHeader(volumePath);
            
            var volume = new Volume(volumePath, blockSize);
            var memory = await volume.ReadBlocks(0, 1);

            var superblock = ParseSuperblock(memory);
            
            return new FileSystem(volumePath, volume, superblock);
        }
        
        public void Unmount(string volumePath)
        {
            if (!File.Exists(volumePath)) throw new VolumeNotFoundException(volumePath);
            
            throw new NotImplementedException();
        }

        private static int ValidateHeader(string volumePath)
        {
            var buffer = new byte[8];
            using (var stream = new FileStream(volumePath, FileMode.Open))
                stream.Read(buffer);

            var magicNumber = BitConverter.ToInt32(buffer, 0);
            var blockSize = BitConverter.ToInt32(buffer, 4);

            if (magicNumber != MagicConstant.SuperblockMagicNumber)
            {
                throw new FileSystemException($"Invalid file system by volume path {volumePath}");
            }

            return blockSize;
        }
        
        private static Superblock ParseSuperblock(ReadOnlyMemory<byte> block)
        {
            using (var ms = new MemoryStream(block.ToArray()))
                return (Superblock) new BinaryFormatter().Deserialize(ms);
        }
    }
}