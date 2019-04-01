using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jbta.VirtualFileSystem.Impl
{
    internal interface IVolumeWriter
    {
        ValueTask WriteBlocksСontiguously(byte[] data, int startBlockNumber, int blocksCount);
        ValueTask WriteBlocks(byte[] data, IReadOnlyList<int> blocksNumbers);
    }
}