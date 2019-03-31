using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Impl;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Mounting
{
    internal class Unmounter
    {
        private readonly IFileSystemMeta _fileSystemMeta;
        private readonly Volume _volume;

        public Unmounter(IFileSystemMeta fileSystemMeta, Volume volume)
        {
            _fileSystemMeta = fileSystemMeta;
            _volume = volume;
        }

        public async Task Unmount()
        {
            var superblock = (Superblock) _fileSystemMeta;
            superblock.IsDirty = false;
            await _volume.WriteBlocks(superblock.Serialize().ToArray(), new[] {0});
        }
    }
}