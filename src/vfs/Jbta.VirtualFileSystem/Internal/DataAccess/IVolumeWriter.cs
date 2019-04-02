using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Internal.DataAccess
{
    internal interface IVolumeWriter
    {
        ValueTask WriteBlock(byte[] data, int blockNumber);
        
        ValueTask WriteBlocks(byte[] data, IReadOnlyList<int> blocksNumbers);
    }
}