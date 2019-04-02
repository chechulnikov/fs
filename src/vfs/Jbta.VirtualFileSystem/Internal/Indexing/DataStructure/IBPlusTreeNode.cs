namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    internal interface IBPlusTreeNode
    {
        bool IsLeaf { get; set; }
            
        int KeysNumber { get; set; }
        
        string[] Keys { get; }
        
        IBPlusTreeNode[] Children { get; }
            
        int[] Pointers { get; }
        
        IBPlusTreeNode Parent { get; set; }
        
        IBPlusTreeNode LeftSibling { get; set; }
            
        IBPlusTreeNode RightSibling { get; set; }
    }
}