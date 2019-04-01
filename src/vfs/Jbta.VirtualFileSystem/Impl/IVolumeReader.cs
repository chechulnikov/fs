using System;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Impl
{
    internal interface IVolumeReader
    {
        ValueTask<Memory<byte>> ReadBlocksToBuffer(Memory<byte> buffer, int startBlockNumber);
        
        ValueTask<byte[]> ReadBlocks(int startBlockNumber, int blocksCount = 1);
    }
}