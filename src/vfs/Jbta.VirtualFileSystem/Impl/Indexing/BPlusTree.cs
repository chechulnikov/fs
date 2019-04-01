using System;
using System.Linq;
using System.Threading;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    /// <summary>
    /// B+-tree
    /// https://neerc.ifmo.ru/wiki/index.php?title=B%2B-дерево
    /// </summary>
    internal class BPlusTree
    {
        private readonly BPlusTreeNodesFactory _nodesFactory;
        private readonly ReaderWriterLockSlim _locker;
        
        public BPlusTree(BPlusTreeNodesFactory nodesFactory, IBPlusTreeNode root)
        {
            _nodesFactory = nodesFactory;
            _locker = new ReaderWriterLockSlim();
            Degree = GlobalConstant.BPlusTreeDegree;
            Root = root;
            Root.IsLeaf = true;
        }

        public int Degree { get; }
        
        public IBPlusTreeNode Root { get; private set; }

        public (int, bool) Search(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            
            using (_locker.ReaderLock())
            {
                var leaf = FindLeaf(key);
                var index = Array.IndexOf(leaf.Keys, key);
                return index < 0 ? (0, false) : (leaf.Pointers[index], true);
            }
        }

        public bool Insert(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            
            using (_locker.UpgradableReaderLock())
            {
                var leaf = FindLeaf(key);
                if (leaf.Keys.Contains(key))
                {
                    return false;
                }

                using (_locker.WriterLock())
                {
                    // find position for new key
                    var position = 0;
                    while (position < leaf.KeysNumber && LessThan(leaf.Keys[position], key))
                    {
                        position++;
                    }
                    
                    // key inserting
                    for (var i = leaf.KeysNumber; i >= position + 1; i--)
                    {
                        leaf.Keys[i] = leaf.Keys[i - 1];
                        leaf.Pointers[i] = leaf.Pointers[i - 1];
                    }
                    leaf.Keys[position] = key;
                    leaf.Pointers[position] = value;
                    leaf.KeysNumber++;

                    if (leaf.KeysNumber == 2 * Degree)
                    {
                        Split(leaf);
                    }

                    return true;
                }
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

        private void Split(IBPlusTreeNode node)
        {
            var newNode = _nodesFactory.New();
            
            // switch right and left siblings pointers
            newNode.RightSibling = node.RightSibling;
            node.RightSibling.LeftSibling = newNode;
            node.RightSibling = newNode;
            newNode.LeftSibling = node;
            
            // move t-1 values and according pointers to newNode
            var midKey = node.Keys[Degree];
            newNode.KeysNumber = Degree - 1;
            node.KeysNumber = Degree;
            for (var i = 0; i < newNode.KeysNumber; i++)
            {
                newNode.Keys[i] = node.Keys[i + Degree + 1];
                newNode.Pointers[i] = node.Pointers[i + Degree + 1];
                newNode.Children[i] = node.Children[i + Degree + 1];
                newNode.Children[newNode.KeysNumber] = node.Children[2 * Degree];
            }
            
            if (node.IsLeaf)
            {
                newNode.KeysNumber++;
                newNode.IsLeaf = true;
                
                // move into newNode element midKey
                for (var i = newNode.KeysNumber - 1; i >= 1; i--)
                {
                    newNode.Keys[i] = newNode.Keys[i - 1];
                    newNode.Pointers[i] = newNode.Pointers[i - 1];
                }
                newNode.Keys[0] = node.Keys[Degree];
                newNode.Pointers[0] = node.Pointers[Degree];
            }
            
            if (node == Root)
            {
                // create new Root node
                Root = _nodesFactory.New();
                Root.Keys[0] = midKey;
                Root.Children[0] = node;
                Root.Children[1] = _nodesFactory.New();
                Root.KeysNumber = 1;
                node.Parent = Root;
                newNode.Parent = Root;
            }
            else
            {
                newNode.Parent = node.Parent;
                var parent = node.Parent;
                
                // find position midKey into parent
                var position = 0;
                while (position < parent.KeysNumber && LessThan(parent.Keys[position], midKey))
                {
                    position++;
                }
                
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

                if (parent.KeysNumber == 2 * Degree)
                {
                    Split(parent);
                }
            }
        }

        private bool LessThan(string a, string b) => string.Compare(a, b, StringComparison.InvariantCulture) < -1;
    }
}