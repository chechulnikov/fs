namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    internal class BPlusTreeNode : IBPlusTreeNode
    {
        public BPlusTreeNode()
        {
            Keys = new string[2 * GlobalConstant.MinBPlusTreeDegree];
            Values = new int[2 * GlobalConstant.MinBPlusTreeDegree];
            Children = new IBPlusTreeNode[2 * GlobalConstant.MinBPlusTreeDegree + 1];
        }
        
        public bool IsLeaf { get; set; }
        
        public int KeysNumber { get; set; }
        
        public string[] Keys { get; }
        
        public IBPlusTreeNode[] Children { get; }
        
        public int[] Values { get; }
        
        public IBPlusTreeNode Parent { get; set; }
        
        public IBPlusTreeNode LeftSibling { get; set; }
        
        public IBPlusTreeNode RightSibling { get; set; }
    }
}