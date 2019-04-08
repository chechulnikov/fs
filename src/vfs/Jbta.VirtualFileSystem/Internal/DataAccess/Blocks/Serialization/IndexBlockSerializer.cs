using System;
using System.Linq;
using System.Text;
using Jbta.VirtualFileSystem.Internal.Utils;

namespace Jbta.VirtualFileSystem.Internal.DataAccess.Blocks.Serialization
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
            foreach (var key in block.Keys.Take(block.KeysNumber))
            {
                var keyLengthInBytes = key.Length * 2;
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, 1), keyLengthInBytes);
                offset += sizeof(byte);
                Encoding.Unicode.GetBytes(key, new Span<byte>(result, offset, keyLengthInBytes));
                offset += keyLengthInBytes;
            }
            foreach (var pointer in block.Values.Take(block.KeysNumber))
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), pointer);
                offset += sizeof(int);
            }
            foreach (var childBlockNumber in block.ChildrenBlockNumbers.Take(block.KeysNumber + 1))
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), childBlockNumber);
                offset += sizeof(int);
            }

            return result;
        }

        public IndexBlock Deserialize(byte[] data)
        {
            var result = new IndexBlock();

            var offset = 0;
            result.IsLeaf = BitConverter.ToBoolean(data, offset);
            result.ParentBlockNumber = BitConverter.ToInt32(data, offset += sizeof(bool));
            result.LeftSiblingBlockNumber = BitConverter.ToInt32(data, offset += sizeof(int));
            result.RightSiblingBlockNumber = BitConverter.ToInt32(data, offset += sizeof(int));
            result.KeysNumber = BitConverter.ToInt32(data, offset += sizeof(int));
            offset += sizeof(int);
            foreach (var i in Enumerable.Range(0, result.KeysNumber))
            {
                var keyLength = data[offset];
                offset += sizeof(byte);
                if (offset == 0)
                {
                    continue;
                }
                
                var key = Encoding.Unicode.GetString(new Span<byte>(data, offset, keyLength));
                result.Keys[i] = key;
                offset += keyLength;
            }
            foreach (var i in Enumerable.Range(0, result.KeysNumber))
            {
                result.Values[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            foreach (var i in Enumerable.Range(0, result.KeysNumber + 1))
            {
                result.ChildrenBlockNumbers[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            
            return result;
        }
    }
}