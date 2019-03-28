using System;

namespace Vfs
{
    /// <summary>
    /// File meta block structure
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
        
        public int[] DirectBlocks { get; set; }
        
        public int[] IndirectBlocks { get; set; }
        
        public byte[] Serialize()
        {
            throw new System.NotImplementedException();
        }
    }
}