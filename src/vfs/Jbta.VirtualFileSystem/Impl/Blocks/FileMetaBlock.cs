using System;
using System.Collections.Generic;

namespace Jbta.VirtualFileSystem.Impl.Blocks
{
    /// <summary>
    /// File meta block structure (aka inode)
    /// </summary>
    internal class FileMetaBlock
    {
        public FileMetaBlock()
        {
            DirectBlocks = new int[GlobalConstant.MaxFileDirectBlocksCount];
            IndirectBlocks = new int[GlobalConstant.MaxFileIndirectBlocksCount];
        }
        
        public int DirectBlocksCount { get; set; }
        
        public int IndirectBlocksCount { get; set; }
        
        public int[] DirectBlocks { get; }
        
        public int[] IndirectBlocks { get; }
    }
}