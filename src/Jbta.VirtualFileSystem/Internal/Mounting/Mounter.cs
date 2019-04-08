using System;
using System.IO;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.Utils;

namespace Jbta.VirtualFileSystem.Internal.Mounting
{
    internal class Mounter
    {
        private readonly IBinarySerializer<Superblock> _superblockSerializer;
        private readonly IBinarySerializer<IndexBlock> _indexBlockSerializer;

        public Mounter(
            IBinarySerializer<Superblock> superblockSerializer,
            IBinarySerializer<IndexBlock> indexBlockSerializer)
        {
            _superblockSerializer = superblockSerializer;
            _indexBlockSerializer = indexBlockSerializer;
        }
        
        public async Task<IFileSystem> Mount(string volumePath)
        {
            var blockSize = ValidateHeader(volumePath);
            
            var volume = new Volume(volumePath, blockSize);
            var superblock = await ReadSuperblock(volume);

            await MarkFileSystemAsDirty(volume, superblock);

            var bitmapBlocks = await volume.ReadBlocks(1, GlobalConstant.BitmapBlocksCount);
            var rootIndexBlock = await ReadRootIndexBlock(volume, superblock);
            
            return FileSystemFactory.New(volume, superblock, bitmapBlocks, rootIndexBlock);
        }

        private static int ValidateHeader(string volumePath)
        {
            var buffer = new byte[3 * sizeof(int) + sizeof(bool)];
            using (var stream = new FileStream(volumePath, FileMode.Open))
            {
                stream.Read(buffer);
            }

            var offset = 0;
            var magicNumber = BitConverter.ToInt32(buffer, offset);
            var isDirty = BitConverter.ToBoolean(buffer, offset += sizeof(int));
            var blockSize = BitConverter.ToInt32(buffer, offset + sizeof(bool));
            
            if (magicNumber != GlobalConstant.SuperblockMagicNumber)
            {
                throw new FileSystemException($"Invalid file system by volume path {volumePath}");
            }
            if (isDirty)
            {
                throw new FileSystemException("File system is already mounted or previous session was failed");
            }

            return blockSize;
        }

        private async ValueTask<Superblock> ReadSuperblock(IVolumeReader volume)
        {
            var superblockData = await volume.ReadBlocks(0, 1);
            return _superblockSerializer.Deserialize(superblockData);
        }

        private async Task<IndexBlock> ReadRootIndexBlock(IVolumeReader volume, Superblock superblock)
        {
            var blockData = await volume.ReadBlocks(superblock.RootIndexBlockNumber + 1);
            return _indexBlockSerializer.Deserialize(blockData);
        }

        private ValueTask MarkFileSystemAsDirty(IVolumeWriter volume, Superblock superblock)
        {
            superblock.IsDirty = !superblock.IsDirty;
            return volume.WriteBlocks(_superblockSerializer.Serialize(superblock), new[] {0});
        }
    }
}