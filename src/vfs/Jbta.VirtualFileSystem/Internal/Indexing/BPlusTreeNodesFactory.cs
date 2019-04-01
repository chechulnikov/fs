using Jbta.VirtualFileSystem.Internal.Blocks;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.Indexing
{
    internal class BPlusTreeNodesFactory
    {
        private readonly IBinarySerializer<IndexBlock> _indexBlockSerializer;

        public BPlusTreeNodesFactory(IBinarySerializer<IndexBlock> indexBlockSerializer)
        {
            _indexBlockSerializer = indexBlockSerializer;
        }
        
        public IBPlusTreeNode New() => new IndexBlock();

        public IBPlusTreeNode From(byte[] block) => _indexBlockSerializer.Deserialize(block);
    }
}