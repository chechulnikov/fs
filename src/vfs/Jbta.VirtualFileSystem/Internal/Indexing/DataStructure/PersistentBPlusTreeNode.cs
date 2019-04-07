using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;

namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    internal class PersistentBPlusTreeNode : IBPlusTreeNode
    {
        private const int MaxKeysPerNode = GlobalConstant.MinBPlusTreeDegree;
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

        public int[] Values => IndexBlock.Pointers;
        
        public IBPlusTreeNode[] Children { get; }

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

//        public async Task<IBPlusTreeNode> GetParent() =>
//            _parent ?? (_parent = await _nodesPersistenceManager.LoadNode(IndexBlock.ParentBlockNumber));
//
//        public Task SetParent(IBPlusTreeNode node) 
//        {
//            _parent = node;
//            return _nodesPersistenceManager.SaveNode(node);
//        }
//        
//        public async Task<IBPlusTreeNode> GetLeftSibling() =>
//            _leftSibling ?? (_leftSibling = await _nodesPersistenceManager.LoadNode(IndexBlock.LeftSiblingBlockNumber));

//        public Task SetLeftSibling(IBPlusTreeNode node) 
//        {
//            _leftSibling = node;
//            return _nodesPersistenceManager.SaveNode(node);
//        }
//        
//        public async Task<IBPlusTreeNode> GetRightSibling() =>
//            _rightSibling ?? (_rightSibling = await _nodesPersistenceManager.LoadNode(IndexBlock.RightSiblingBlockNumber));
//
//        public Task SetRightSibling(IBPlusTreeNode node) 
//        {
//            _rightSibling = node;
//            return _nodesPersistenceManager.SaveNode(node);
//        }
//
//        private async Task Save()
//        {
//            
//        }
    }
}