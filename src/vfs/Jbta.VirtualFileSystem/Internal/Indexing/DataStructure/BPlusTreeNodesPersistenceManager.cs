using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;
using Jbta.VirtualFileSystem.Internal.SpaceManagement;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    internal class BPlusTreeNodesPersistenceManager
    {
        private readonly BlocksAllocator _allocator;
        private readonly BlocksDeallocator _deallocator;
        private readonly IBinarySerializer<IndexBlock> _indexBlockSerializer;
        private readonly IVolumeReader _volumeReader;
        private readonly IVolumeWriter _volumeWriter;

        public BPlusTreeNodesPersistenceManager(
            BlocksAllocator allocator,
            BlocksDeallocator deallocator,
            IBinarySerializer<IndexBlock> indexBlockSerializer,
            IVolumeReader volumeReader,
            IVolumeWriter volumeWriter)
        {
            _allocator = allocator;
            _deallocator = deallocator;
            _indexBlockSerializer = indexBlockSerializer;
            _volumeReader = volumeReader;
            _volumeWriter = volumeWriter;
        }
        
        public IBPlusTreeNode CreateFrom(IndexBlock indexBlock, int blockNumber)
        {
            return new PersistentBPlusTreeNode(this, indexBlock, blockNumber);
        }
        
        public async Task<IBPlusTreeNode> CreateNewNode()
        {
            var indexBlock = new IndexBlock();
            var indexBlockData = _indexBlockSerializer.Serialize(indexBlock);
            var blockNumber = await _allocator.AllocateBlock();
            await _volumeWriter.WriteBlock(indexBlockData, blockNumber);
            return CreateFrom(indexBlock, blockNumber);
        }
        
        public async Task<IBPlusTreeNode> LoadNode(int blockNumber)
        {
            var blockData = await _volumeReader.ReadBlocks(blockNumber);
            var indexBlock = _indexBlockSerializer.Deserialize(blockData);
            return CreateFrom(indexBlock, blockNumber);
        }
        
        public async Task SaveNode(IBPlusTreeNode node)
        {
            var persistentBPlusTreeNode = (PersistentBPlusTreeNode) node;
            var indexBlock = persistentBPlusTreeNode.IndexBlock;
            var indexBlockData = _indexBlockSerializer.Serialize(indexBlock);
            await _volumeWriter.WriteBlock(indexBlockData, persistentBPlusTreeNode.BlockNumber);
        }

        public async Task DeleteNode(IBPlusTreeNode node)
        {
            var persistentBPlusTreeNode = (PersistentBPlusTreeNode) node;
            await _deallocator.DeallocateBlock(persistentBPlusTreeNode.BlockNumber);
        }
    }
}