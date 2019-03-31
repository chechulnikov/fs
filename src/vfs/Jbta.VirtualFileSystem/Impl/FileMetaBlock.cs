using System;
using System.Collections.Generic;

namespace Jbta.VirtualFileSystem.Impl
{
    /// <summary>
    /// File meta block structure (aka inode)
    /// </summary>
    internal class FileMetaBlock : IBinarySerializable
    {
        public FileMetaBlock()
        {
            FileId = Guid.NewGuid();
        }
        
        public Guid FileId { get; }
        
        public bool InUnderReading { get; set; }
        
        public bool InUnderWriting { get; set; }
        
        public IList<int> DirectBlocks { get; }
        
        public IList<int> IndirectBlocks { get; }
        
        public int CalcDataBlocksSizeInBytes(int blockSize)
        {
            var directBlocksCount = DirectBlocks.Count;
            var indirectBlocksCount = IndirectBlocks.Count;
            var indirectBlockCapacity = blockSize / sizeof(int);
            return (directBlocksCount + indirectBlocksCount * indirectBlockCapacity) * blockSize;
        }
        
        public int CalcDataBlocksCount(int blockSize)
        {
            var directBlocksCount = DirectBlocks.Count;
            var indirectBlocksCount = IndirectBlocks.Count;
            var indirectBlockCapacity = blockSize / sizeof(int);
            return directBlocksCount + indirectBlocksCount * indirectBlockCapacity;
        }
        
        public byte[] Serialize()
        {
            throw new System.NotImplementedException();
        }
    }
}