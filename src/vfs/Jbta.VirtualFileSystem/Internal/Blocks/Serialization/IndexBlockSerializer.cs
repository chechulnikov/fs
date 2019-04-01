using System;
using System.Linq;
using System.Text;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.Blocks.Serialization
{
    internal class IndexBlockSerializer : IBinarySerializer<IndexBlock>
    {
        private readonly int _blockSize;

        public IndexBlockSerializer(int blockSize)
        {
            _blockSize = blockSize;
        }
        
        public byte[] Serialize(IndexBlock block)
        {
            var result = new byte[_blockSize];
            
            var offset = 0;
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.BlockNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(bool)), block.IsLeaf);
            offset += sizeof(bool);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.ParentBlockNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.LeftSiblingBlockNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.RightSiblingBlockNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), block.KeysNumber);
            offset += sizeof(int);
            foreach (var key in block.Keys)
            {
                Encoding.Unicode.GetBytes(key, new Span<byte>(result, offset, GlobalConstant.MaxFileNameSizeInBytes));
                offset += GlobalConstant.MaxFileNameSizeInBytes;
            }
            foreach (var childBlockNumber in block.ChildrenBlockNumbers)
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), childBlockNumber);
                offset += sizeof(int);
            }
            foreach (var pointer in block.Pointers)
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), pointer);
                offset += sizeof(int);
            }

            return result;
        }

        public IndexBlock Deserialize(byte[] data)
        {
            var result = new IndexBlock();

            var offset = 0;
            result.BlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.IsLeaf = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            result.Parent.BlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.LeftSiblingBlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.RightSiblingBlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.KeysNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            foreach (var i in Enumerable.Range(0, result.KeysNumber))
            {
                var key = Encoding.Unicode.GetString(new Span<byte>(data, offset, GlobalConstant.MaxFileNameSizeInBytes));
                result.Keys[i] = key;
                offset += GlobalConstant.MaxFileNameSizeInBytes;
            }
            foreach (var i in Enumerable.Range(0, result.KeysNumber + 1))
            {
                result.ChildrenBlockNumbers[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            foreach (var i in Enumerable.Range(0, result.KeysNumber))
            {
                result.Pointers[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            
            return result;
        }
    }
}