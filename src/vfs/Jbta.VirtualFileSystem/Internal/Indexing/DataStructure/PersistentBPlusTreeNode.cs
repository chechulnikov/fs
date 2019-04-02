using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;

namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    internal class PersistentBPlusTreeNode : IBPlusTreeNode
    {
        private const int MaxKeysPerNode = GlobalConstant.BPlusTreeDegree;
        private readonly BPlusTreeNodesPersistenceManager _nodesPersistenceManager;
        private IBPlusTreeNode _parent;
        private IBPlusTreeNode _leftSibling;
        private IBPlusTreeNode _rightSibling;

        public PersistentBPlusTreeNode(
            BPlusTreeNodesPersistenceManager nodesPersistenceManager,
            IndexBlock indexIndexBlock,
            int blockNumber)
        {
            _nodesPersistenceManager = nodesPersistenceManager;
            Children = new IBPlusTreeNode[MaxKeysPerNode + 1];
            IndexBlock = indexIndexBlock;
            BlockNumber = blockNumber;
        }

        public IndexBlock IndexBlock { get; }

        public int BlockNumber { get; }

        public bool IsLeaf
        {
            get => IndexBlock.IsLeaf;
            set => IndexBlock.IsLeaf = value;
        }

        public int KeysNumber
        {
            get => IndexBlock.KeysNumber;
            set => IndexBlock.KeysNumber = value;
        }

        public string[] Keys => IndexBlock.Keys;

        public IBPlusTreeNode[] Children { get; }

        public int[] Pointers => IndexBlock.Pointers;

        public IBPlusTreeNode Parent
        {
            get => _parent ?? (_parent = _nodesPersistenceManager.LoadNode(IndexBlock.ParentBlockNumber).Result);
            set => _parent = value;
        }

        public IBPlusTreeNode LeftSibling
        {
            get => _leftSibling ?? (_leftSibling = _nodesPersistenceManager.LoadNode(IndexBlock.LeftSiblingBlockNumber).Result);
            set => _leftSibling = value;
        }

        public IBPlusTreeNode RightSibling
        {
            get => _rightSibling ?? (_rightSibling = _nodesPersistenceManager.LoadNode(IndexBlock.RightSiblingBlockNumber).Result);
            set => _rightSibling = value;
        }
    }
}