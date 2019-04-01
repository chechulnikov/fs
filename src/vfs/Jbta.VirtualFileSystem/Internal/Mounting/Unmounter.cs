using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.Blocks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.Mounting
{
    internal class Unmounter
    {
        private readonly IBinarySerializer<Superblock> _superblockSerializer;
        private readonly Superblock _superblock;
        private readonly Volume _volume;

        public Unmounter(IBinarySerializer<Superblock> superblockSerializer, Superblock superblock, Volume volume)
        {
            _superblockSerializer = superblockSerializer;
            _superblock = superblock;
            _volume = volume;
        }

        public async Task Unmount()
        {
            _superblock.IsDirty = false;
            var superblockData = _superblockSerializer.Serialize(_superblock);
            await _volume.WriteBlocks(superblockData, new[] {0});
        }
    }
}