using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Impl;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Mounting
{
    internal class Unmounter
    {
        private readonly Superblock _superblock;
        private readonly Volume _volume;

        public Unmounter(Superblock superblock, Volume volume)
        {
            _superblock = superblock;
            _volume = volume;
        }

        public async Task Unmount()
        {
            _superblock.IsDirty = false;
            var superblockData = _superblock.Serialize().ToArray();
            await _volume.WriteBlocks(superblockData, new[] {0});
        }
    }
}