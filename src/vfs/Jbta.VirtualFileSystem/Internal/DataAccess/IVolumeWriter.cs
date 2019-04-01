using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Internal.DataAccess
{
    internal interface IVolumeWriter
    {
        ValueTask WriteBlocksСontiguously(byte[] data, int startBlockNumber, int blocksCount);
        
        ValueTask WriteBlocks(byte[] data, IReadOnlyList<int> blocksNumbers);
    }
}