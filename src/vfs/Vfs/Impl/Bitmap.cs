using System;
using System.Collections;

namespace Vfs
{
    /// <summary>
    /// Tree like
    /// 1 1 1 0 1 0 0 1
    ///  1   0   0   0
    ///    0       0
    /// </summary>
    internal class BitmapTree
    {
        private readonly int _dataLength;
        private readonly BitArray _tree;
        
        public BitmapTree(byte[] data)
        {
            var bitmapData = new BitArray(data);
            
            _dataLength = bitmapData.Length;
            _tree = new BitArray(2 * bitmapData.Length);
            
            for (var i = 0; i < bitmapData.Length; i++)
                _tree[bitmapData.Length + i] = bitmapData[i];
            
            for (var i = bitmapData.Length - 1; i >= 0; i--)
                _tree[i] = TreeOperation(_tree[2 * i], _tree[2 * i + 1]);
        }
        
        // boolean min
        private static bool TreeOperation(bool first, bool second) => first && second;
        
        public int GetNumberOfFirstUnsetBit()
        {
            var position = 1;
            while (position <= 2 * _dataLength)
            {
                position *= 2;
                
                var left = _tree[position];
                var right = _tree[position + 1];
                
                if (!left) continue;
                if (!right) throw new FileSystemException("Invalid bitmap tree state");

                position += 1;
            }

            return position - _dataLength;
        }

        public bool this[int blockNumber]
        {
            get => _tree[_dataLength + blockNumber];
            set
            {
                var position = _dataLength + blockNumber;
                var divisor = 1;
                while (position >= 1)
                {
                    _tree[position] = value;
                    
                    divisor *= 2;
                    position /= divisor;
                }
                
            }
        }
    }
}