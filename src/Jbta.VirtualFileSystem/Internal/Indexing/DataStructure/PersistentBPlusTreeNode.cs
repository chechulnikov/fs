using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;

namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    internal class PersistentBPlusTreeNode : IBPlusTreeNode
    {
        private readonly IBPlusTreeNodesPersistenceManager _nodesPersistenceManager;
        private IBPlusTreeNode _parent;
        private IBPlusTreeNode _leftSibling;
        private IBPlusTreeNode _rightSibling;
        private IBPlusTreeNode[] _children;

        public PersistentBPlusTreeNode(
            IBPlusTreeNodesPersistenceManager nodesPersistenceManager,
            IndexBlock indexIndexBlock,
            int blockNumber)
        {
            _nodesPersistenceManager = nodesPersistenceManager;
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

        public int[] Values => IndexBlock.Values;

        public IBPlusTreeNode[] Children => _children ?? (_children = LoadChildren());

        public IBPlusTreeNode Parent
        {
            get => _parent ?? (_parent = LoadNode(IndexBlock.ParentBlockNumber));
            set => _parent = value;
        }

        public IBPlusTreeNode LeftSibling
        {
            get => _leftSibling ?? (_leftSibling = LoadNode(IndexBlock.LeftSiblingBlockNumber));
            set => _leftSibling = value;
        }

        public IBPlusTreeNode RightSibling
        {
            get => _rightSibling ?? (_rightSibling = LoadNode(IndexBlock.RightSiblingBlockNumber));
            set => _rightSibling = value;
        }

        private IBPlusTreeNode LoadNode(int blockNumber) =>
            blockNumber == 0 ? null : Task.Run(async () => await _nodesPersistenceManager.LoadNode(blockNumber)).Result;

        private IBPlusTreeNode[] LoadChildren()
        {
            var result = new IBPlusTreeNode[2 * GlobalConstant.MinBPlusTreeDegree + 1];
            var blockNumbers = IndexBlock.ChildrenBlockNumbers.Take(IndexBlock.KeysNumber + 1).ToArray();
            for (var i = 0; i < blockNumbers.Length; i++)
            {
                result[i] = LoadNode(blockNumbers[i]);
            }

            return result;
        }
    }
}