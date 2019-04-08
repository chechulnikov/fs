namespace Jbta.VirtualFileSystem.Internal.DataAccess.Blocks
{
    internal class IndexBlock
    {
        public IndexBlock()
        {
            Keys = new string[2 * GlobalConstant.MinBPlusTreeDegree];
            Values = new int[2 * GlobalConstant.MinBPlusTreeDegree];
            ChildrenBlockNumbers = new int[2 * GlobalConstant.MinBPlusTreeDegree + 1];
        }
        
        public bool IsLeaf { get; set; }
        
        public int KeysNumber { get; set; }
        
        public string[] Keys { get; set; }
        
        public int[] ChildrenBlockNumbers { get; set; }
        
        public int[] Values { get; set; }
        
        public int ParentBlockNumber { get; set; }
        
        public int LeftSiblingBlockNumber { get; set; }
        
        public int RightSiblingBlockNumber { get; set; }
    }
}