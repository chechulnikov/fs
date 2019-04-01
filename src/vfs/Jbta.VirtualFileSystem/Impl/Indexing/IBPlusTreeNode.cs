namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    public interface IBPlusTreeNode
    {
        int BlockNumber { get; set; }
        
        bool IsLeaf { get; set; }
            
        int KeysNumber { get; set; }
            
        string[] Keys { get; set; }
            
        IBPlusTreeNode Parent { get; set; }
            
        IBPlusTreeNode[] Children { get; set; }
            
        int[] Pointers { get; set; }
            
        IBPlusTreeNode LeftSibling { get; set; }
            
        IBPlusTreeNode RightSibling { get; set; }
    }
}