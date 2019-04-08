using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    /// <summary>
    /// B+-tree
    /// https://neerc.ifmo.ru/wiki/index.php?title=B%2B-дерево
    /// </summary>
    internal class BPlusTree
    {
        private readonly IBPlusTreeNodesPersistenceManager _nodesPersistenceManager;
        private readonly AsyncReaderWriterLock _locker;
        private readonly ISet<IBPlusTreeNode> _modifiedNodes;
        private readonly ISet<IBPlusTreeNode> _deletedNodes;
        private readonly int _degree;

        public BPlusTree(IBPlusTreeNodesPersistenceManager nodesPersistenceManager, IBPlusTreeNode root)
        {
            _nodesPersistenceManager = nodesPersistenceManager;
            _locker = new AsyncReaderWriterLock();
            _modifiedNodes = new HashSet<IBPlusTreeNode>();
            _deletedNodes = new HashSet<IBPlusTreeNode>();
            _degree = GlobalConstant.MinBPlusTreeDegree;
            Root = root;
            Root.IsLeaf = true;
        }

        public IBPlusTreeNode Root { get; private set; }

        public (int, bool) Search(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            using (_locker.ReaderLock())
            {
                var leaf = FindLeaf(key);
                var index = Array.IndexOf(leaf.Keys, key);
                return index < 0 ? (0, false) : (leaf.Values[index], true);
            }
        }

        public async Task<bool> Insert(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            
            using (_locker.WriterLock())
            {
                var leaf = FindLeaf(key);
                if (leaf.Keys.Contains(key))
                {
                    return false;
                }

                // find position for new key
                var position = CalcPosition(leaf, key);
                
                // key inserting
                for (var i = leaf.KeysNumber; i >= position + 1; i--)
                {
                    leaf.Keys[i] = leaf.Keys[i - 1];
                    leaf.Values[i] = leaf.Values[i - 1];
                }
                leaf.Keys[position] = key;
                leaf.Values[position] = value;
                leaf.KeysNumber++;
                
                _modifiedNodes.Add(leaf);

                if (leaf.KeysNumber == 2 * _degree)
                {
                    await Split(leaf);
                }
                
                await Persist();
                return true;
            }
        }
        
        public async Task<bool> Delete(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            
            using (_locker.WriterLock())
            {
                var leaf = FindLeaf(key);
                if (!leaf.Keys.Contains(key))
                {
                    return false;
                }

                await DeleteInNode(leaf, key);
                
                await Persist();
                return true;
            }
        }

        private IBPlusTreeNode FindLeaf(string key)
        {
            var current = Root;
            while (!current.IsLeaf)
            {
                for (var i = 0; i <= current.KeysNumber; i++)
                {
                    if (i == current.KeysNumber || LessThan(key, current.Keys[i]))
                    {
                        current = current.Children[i];
                        break;
                    }
                }
            }
            return current;
        }

        private async Task Split(IBPlusTreeNode node)
        {
            var newNode = await _nodesPersistenceManager.CreateNewNode();
            
            // switch right and left siblings pointers
            newNode.RightSibling = node.RightSibling;
            if (node.RightSibling != null)
                node.RightSibling.LeftSibling = newNode;
            node.RightSibling = newNode;
            newNode.LeftSibling = node;
            
            // move Degree-1 values and according pointers to newNode
            var midKey = node.Keys[_degree];
            newNode.KeysNumber = _degree - 1;
            node.KeysNumber = _degree;
            for (var i = 0; i < newNode.KeysNumber; i++)
            {
                newNode.Keys[i] = node.Keys[i + _degree + 1];
                newNode.Values[i] = node.Values[i + _degree + 1];
                newNode.Children[i] = node.Children[i + _degree + 1];
                node.Keys[i + _degree + 1] = null;
                node.Values[i + _degree + 1] = 0;
                node.Children[i + _degree + 1] = null;
            }
            newNode.Children[newNode.KeysNumber] = node.Children[2 * _degree];
            node.Children[2 * _degree] = null;
            
            if (node.IsLeaf)
            {
                newNode.IsLeaf = true;
                
                // move into newNode element midKey
                for (var i = newNode.KeysNumber; i >= 1; i--)
                {
                    newNode.Keys[i] = newNode.Keys[i - 1];
                    newNode.Values[i] = newNode.Values[i - 1];
                }
                newNode.Keys[0] = node.Keys[_degree];
                newNode.Values[0] = node.Values[_degree];
                newNode.KeysNumber++;
            }
            
            node.Keys[_degree] = null;
            node.Values[_degree] = 0;
            
            if (node == Root)
            {
                // create new Root node
                Root = await _nodesPersistenceManager.CreateNewNode();
                Root.Keys[0] = midKey;
                Root.Children[0] = node;
                Root.Children[1] = newNode;
                Root.KeysNumber = 1;
                node.Parent = Root;
                newNode.Parent = Root;
                
                _modifiedNodes.Add(Root);
            }
            else
            {
                newNode.Parent = node.Parent;
                var parent = node.Parent;
                
                // find position midKey into parent
                var position = CalcPosition(parent, midKey);
                
                // add midKey into parent and wire reference from it to newNode
                for (var i = parent.KeysNumber; i >= position + 1; i--)
                {
                    parent.Keys[i] = parent.Keys[i - 1];
                }
                for (var i = parent.KeysNumber + 1; i >= position + 2; i--)
                {
                    parent.Children[i] = parent.Children[i - 1];
                }

                parent.Keys[position] = midKey;
                parent.Children[position + 1] = newNode;
                parent.KeysNumber++;

                if (parent.KeysNumber == 2 * _degree)
                {
                    await Split(parent);
                }
            }
            
            _modifiedNodes.Add(node);
        }

        private static bool LessThan(string a, string b) =>
            string.Compare(a, b, StringComparison.InvariantCulture) < 0;

        private async Task DeleteInNode(IBPlusTreeNode node, string key)
        {
            if (node == null || !node.Keys.Contains(key))
            {
                return;
            }
                
            // delete key
            var position = CalcPosition(node, key);
            for (var i = position; i < node.KeysNumber; i++)
            {
                node.Keys[i] = node.Keys[i + 1];
                node.Values[i] = node.Values[i + 1];
            }
            for (var i = position + 1; i <= node.KeysNumber; i++)
            {
                node.Children[i] = node.Children[i + 1];
            }
            node.Keys[node.KeysNumber - 1] = null;
            node.Values[node.KeysNumber - 1] = 0;
            node.Children[node.KeysNumber] = null;
            node.KeysNumber--;
            
            _modifiedNodes.Add(node);

            if (position == 0)
            {
                // update key in parent
                UpdateKeysOnTheWayToRoot(node, key);
            }

            // need to balance?
            if (node.KeysNumber < _degree)
            {
                await BalanceAfterDeletion(node, key);
            }
        }

        private async Task BalanceAfterDeletion(IBPlusTreeNode node, string key)
        {
            var rightSibling = node.RightSibling;
            var leftSibling = node.LeftSibling;

            // move min key from left sibling
            if (leftSibling != null && leftSibling.KeysNumber > _degree)
            {
                MoveMaxKeyFromLeftSibling(node);
            }
            // ...or move max key from right one
            else if (rightSibling != null && rightSibling.KeysNumber > _degree)
            {
                MoveMinKeyFromRightSibling(node);
            }
            // ...or remove node
            else await DeleteNode(node, key);
            
            TryToShrinkTreeHeight();
        }

        // move max key from leftSibling to the first position in node and updates parents
        private void MoveMaxKeyFromLeftSibling(IBPlusTreeNode node)
        {
            var leftSibling = node.LeftSibling;
            var outdatedKey = node.Keys[0];

            node.Children[node.KeysNumber + 1] = node.Children[node.KeysNumber];
            for (var i = node.KeysNumber; i >= 1; i--)
            {
                node.Keys[i] = node.Keys[i - 1];
                node.Values[i] = node.Values[i - 1];
                node.Children[i] = node.Children[i - 1];
            }
            node.KeysNumber++;
            
            node.Keys[0] = leftSibling.Keys[leftSibling.KeysNumber - 1];
            node.Values[0] = leftSibling.Values[leftSibling.KeysNumber - 1];
            node.Children[0] = leftSibling.Children[leftSibling.KeysNumber];

            leftSibling.Keys[leftSibling.KeysNumber - 1] = null;
            leftSibling.Values[leftSibling.KeysNumber - 1] = 0;
            leftSibling.Children[leftSibling.KeysNumber] = null;
            leftSibling.KeysNumber--;

            _modifiedNodes.Add(leftSibling);
            _modifiedNodes.Add(node);
            
            UpdateKeysOnTheWayToRoot(node, outdatedKey);
        }

        // move min key from rightSibling on the last position in node and updates parents
        private void MoveMinKeyFromRightSibling(IBPlusTreeNode node)
        {
            var rightSibling = node.RightSibling;
            var outdatedKey = rightSibling.Keys[0];

            node.Keys[node.KeysNumber] = rightSibling.Keys[0];
            node.Values[node.KeysNumber] = rightSibling.Values[0];
            node.Children[node.KeysNumber + 1] = rightSibling.Children[0];
            node.KeysNumber++;
            
            for (var i = 1; i < rightSibling.KeysNumber; i++)
            {
                rightSibling.Keys[i - 1] = rightSibling.Keys[i];
                rightSibling.Values[i - 1] = rightSibling.Values[i];
                rightSibling.Children[i - 1] = rightSibling.Children[i];
            }
            rightSibling.Children[rightSibling.KeysNumber - 1] = rightSibling.Children[rightSibling.KeysNumber];
            
            rightSibling.Keys[rightSibling.KeysNumber - 1] = null;
            rightSibling.Values[rightSibling.KeysNumber - 1] = 0;
            rightSibling.Children[rightSibling.KeysNumber] = null;
            rightSibling.KeysNumber--;
            
            _modifiedNodes.Add(rightSibling);
            _modifiedNodes.Add(node);
            
            UpdateKeysOnTheWayToRoot(node, outdatedKey);
        }

        private async Task DeleteNode(IBPlusTreeNode node, string key)
        {
            var rightSibling = node.RightSibling;
            var leftSibling = node.LeftSibling;
            
            if (leftSibling != null)
            {
                // merge node and leftSibling
                for (var i = 0; i < node.KeysNumber; i++)
                {
                    leftSibling.Keys[leftSibling.KeysNumber] = node.Keys[i];
                    leftSibling.Values[leftSibling.KeysNumber] = node.Values[i];
                    leftSibling.Children[leftSibling.KeysNumber + 1] = node.Children[i];
                    leftSibling.KeysNumber++;
                }
                leftSibling.Children[leftSibling.KeysNumber + 1] = node.Children[node.KeysNumber];

                // swap right and left pointers
                leftSibling.RightSibling = node.RightSibling;
                if (node.RightSibling != null)
                {
                    node.RightSibling.LeftSibling = leftSibling;
                }

                _deletedNodes.Add(node);
                _modifiedNodes.Add(leftSibling);

                //UpdateKeysOnTheWayToRoot(leftSibling, outdatedKey);
                var min = node.Keys.Min();
                await DeleteInNode(leftSibling.Parent, min);
            } else if (rightSibling != null)
            {
                // merge node and rightSibling
                for (var i = 0; i < rightSibling.KeysNumber; i++)
                {
                    node.Keys[node.KeysNumber] = rightSibling.Keys[i];
                    node.Values[node.KeysNumber] = rightSibling.Values[i];
                    node.Children[node.KeysNumber + 1] = rightSibling.Children[i];
                    node.KeysNumber++;
                }
                node.Children[node.KeysNumber + 1] = rightSibling.Children[rightSibling.KeysNumber];

                // swap right and left pointers
                if (rightSibling.RightSibling != null)
                {
                    rightSibling.RightSibling.LeftSibling = node;
                }
                node.RightSibling = rightSibling.RightSibling;

                _deletedNodes.Add(rightSibling);
                _modifiedNodes.Add(node);

                //UpdateKeysOnTheWayToRoot(node, outdatedKey);
                var min = rightSibling.Keys.Min();
                await DeleteInNode(node.Parent, min);
            }
        }

        private void UpdateKeysOnTheWayToRoot(IBPlusTreeNode node, string outdatedKey)
        {
            while (true)
            {
                if (node == null)
                {
                    return;
                }

                if (node.IsLeaf)
                {
                    node = node.Parent;
                    continue;
                }

                for (var i = 0; i < node.KeysNumber; i++)
                {
                    if (string.Compare(node.Keys[i], outdatedKey, StringComparison.InvariantCulture) != 0)
                    {
                        continue;
                    }
                    
                    node.Keys[i] = FindMinNode(node.Children[i + 1]).Keys[0];
                    break;
                }

                _modifiedNodes.Add(node);
                
                node = node.Parent;
            }
        }

        // return the node containing min value which will be a leaf at the left most
        private static IBPlusTreeNode FindMinNode(IBPlusTreeNode node)
        {
            while (true)
            {
                if (node.IsLeaf) return node;
                node = node.Children[0];
            }
        }

        private static int CalcPosition(IBPlusTreeNode node, string key)
        {
            var position = 0;
            while (position < node.KeysNumber && LessThan(node.Keys[position], key))
            {
                position++;
            }
            return position;
        }

        private void TryToShrinkTreeHeight()
        {
            if (Root.KeysNumber != 1)
            {
                return;
            }

            var newRoot = Root.Children[0];
            if (newRoot == null) return;
            newRoot.Parent = null;
            Root = newRoot;
        }

        private async Task Persist()
        {
            await _nodesPersistenceManager.UpdateNodes(_modifiedNodes);
            await _nodesPersistenceManager.DeleteNodes(_deletedNodes);
            _modifiedNodes.Clear();
            _deletedNodes.Clear();
        }
    }
}