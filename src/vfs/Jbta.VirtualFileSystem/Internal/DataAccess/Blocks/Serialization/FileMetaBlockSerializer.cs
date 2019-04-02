using System;
using System.Linq;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.DataAccess.Blocks.Serialization
{
    internal class FileMetaBlockSerializer : IBinarySerializer<FileMetaBlock>
    {
        private readonly int _blockSize;

        public FileMetaBlockSerializer(int blockSize)
        {
            _blockSize = blockSize;
        }
        
        public byte[] Serialize(FileMetaBlock block)
        {
            var result = new byte[_blockSize];
            
            var offset = 0;
            foreach (var directBlockNumber in block.DirectBlocks)
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), directBlockNumber);
                offset += sizeof(int);
            }
            foreach (var indirectBlockNumber in block.IndirectBlocks)
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), indirectBlockNumber);
                offset += sizeof(int);
            }

            return result;
        }

        public FileMetaBlock Deserialize(byte[] data)
        {
            var result = new FileMetaBlock();

            var offset = 0;
            foreach (var i in Enumerable.Range(0, result.DirectBlocks.Length))
            {
                result.DirectBlocks[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            foreach (var i in Enumerable.Range(0, result.IndirectBlocks.Length))
            {
                result.DirectBlocks[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            
            return result;
        }
    }
}