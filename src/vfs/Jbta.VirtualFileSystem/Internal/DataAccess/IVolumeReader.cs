using System;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Internal.DataAccess
{
    internal interface IVolumeReader
    {
        ValueTask<Memory<byte>> ReadBlocksToBuffer(Memory<byte> buffer, int startBlockNumber);
        
        ValueTask<byte[]> ReadBlocks(int startBlockNumber, int blocksCount = 1);
    }
}