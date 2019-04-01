using Jbta.VirtualFileSystem.Internal.Indexing;

namespace Jbta.VirtualFileSystem.Internal.Blocks
{
    internal class IndexBlock : IBPlusTreeNode
    {
        private const int MaxKeysPerNode = GlobalConstant.BPlusTreeDegree;

        public IndexBlock()
        {
            Keys = new string[MaxKeysPerNode];
            ChildrenBlockNumbers = new int[MaxKeysPerNode];
            Children = new IBPlusTreeNode[MaxKeysPerNode + 1];
            Pointers = new int[MaxKeysPerNode];
        }
        
        public int BlockNumber { get; set; }
            
        public bool IsLeaf { get; set; }
        
        public int ParentBlockNumber { get; set; }
        
        public int LeftSiblingBlockNumber { get; set; }
        
        public int RightSiblingBlockNumber { get; set; }
        
        public int KeysNumber { get; set; }
        
        public string[] Keys { get; set; }
        
        public int[] ChildrenBlockNumbers { get; set; }
        
        public int[] Pointers { get; set; }
        
        public IBPlusTreeNode[] Children { get; set; }
        
        public IBPlusTreeNode Parent { get; set; }
        
        public IBPlusTreeNode LeftSibling { get; set; }
        
        public IBPlusTreeNode RightSibling { get; set; }
    }
}