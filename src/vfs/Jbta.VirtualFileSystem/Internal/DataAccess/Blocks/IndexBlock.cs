namespace Jbta.VirtualFileSystem.Internal.DataAccess.Blocks
{
    internal class IndexBlock
    {
        public IndexBlock()
        {
            Keys = new string[GlobalConstant.BPlusTreeDegree];
            ChildrenBlockNumbers = new int[GlobalConstant.BPlusTreeDegree + 1];
            Pointers = new int[GlobalConstant.BPlusTreeDegree];
        }
        
        public bool IsLeaf { get; set; }
        
        public int KeysNumber { get; set; }
        
        public string[] Keys { get; set; }
        
        public int[] ChildrenBlockNumbers { get; set; }
        
        public int[] Pointers { get; set; }
        
        public int ParentBlockNumber { get; set; }
        
        public int LeftSiblingBlockNumber { get; set; }
        
        public int RightSiblingBlockNumber { get; set; }
    }
}