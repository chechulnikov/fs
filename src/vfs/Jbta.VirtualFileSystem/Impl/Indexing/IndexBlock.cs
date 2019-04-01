using System;
using System.Linq;
using System.Text;

namespace Jbta.VirtualFileSystem.Impl.Indexing
{
    internal class IndexBlock : IBPlusTreeNode, IBinarySerializable
    {
        private readonly int _blockSizeInBytes;
        private const int MaxKeysPerNode = GlobalConstant.BPlusTreeDegree;

        public IndexBlock(int blockSizeInBytes)
        {
            _blockSizeInBytes = blockSizeInBytes;
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
        
        public byte[] Serialize()
        {
            var result = new byte[_blockSizeInBytes];
            
            var offset = 0;
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), BlockNumber);
            offset += sizeof(int) - 1;
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(bool)), IsLeaf);
            offset += sizeof(bool);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), ParentBlockNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), LeftSiblingBlockNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), RightSiblingBlockNumber);
            offset += sizeof(int);
            BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), KeysNumber);
            offset += sizeof(int);
            foreach (var key in Keys)
            {
                Encoding.Unicode.GetBytes(key, new Span<byte>(result, offset, GlobalConstant.MaxFileNameSizeInBytes));
                offset += GlobalConstant.MaxFileNameSizeInBytes;
            }
            foreach (var childBlockNumber in ChildrenBlockNumbers)
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), childBlockNumber);
                offset += sizeof(int);
            }
            foreach (var pointer in Pointers)
            {
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, sizeof(int)), pointer);
                offset += sizeof(int);
            }

            return result;
        }
        
        public static IndexBlock Deserialize(byte[] data)
        {
            var result = new IndexBlock(data.Length);

            var offset = 0;
            result.BlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.IsLeaf = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            result.Parent.BlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.LeftSiblingBlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.RightSiblingBlockNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            result.KeysNumber = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            foreach (var i in Enumerable.Range(0, result.KeysNumber))
            {
                var key = Encoding.Unicode.GetString(new Span<byte>(data, offset, GlobalConstant.MaxFileNameSizeInBytes));
                result.Keys[i] = key;
                offset += GlobalConstant.MaxFileNameSizeInBytes;
            }
            foreach (var i in Enumerable.Range(0, result.KeysNumber + 1))
            {
                result.ChildrenBlockNumbers[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            foreach (var i in Enumerable.Range(0, result.KeysNumber))
            {
                result.Pointers[i] = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
            }
            
            return result;
        }
    }
}