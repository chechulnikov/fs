using System;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.Blocks.Serialization
{
    internal class SuperblockSerializer : IBinarySerializer<Superblock>
    {
        private readonly int _blockSize;

        public SuperblockSerializer(int blockSize)
        {
            _blockSize = blockSize;
        }
        
        public byte[] Serialize(Superblock block)
        {
            var result = new byte[_blockSize];
            var offset = 0;
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.MagicNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(bool)), block.IsDirty);
            offset += sizeof(bool);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.BlockSize);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.RootIndexBlockNumber);
            return result;
        }

        public Superblock Deserialize(byte[] data)
        {
            var result = new Superblock();
            var offset = 0;
            result.MagicNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.IsDirty = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            result.BlockSize = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.RootIndexBlockNumber = BitConverter.ToInt32(data, offset);
            return result;
        }
    }
}