using System.Collections;
using System.Collections.Generic;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.SpaceManagement
{
    /// <summary>
    /// This is a tree that provides first unset bit search search for O(log n)
    /// Example:
    ///    0       0
    ///  1   0   0   0
    /// 1 1 1 0 1 0 0 1
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

            SetBitsCount = CountUnsetBits();
        }

        public int SetBitsCount { get; private set; }
        
        public bool TrySetBit(int bitNumber)
        {
           if (this[bitNumber]) return false;
           SetBit(bitNumber);
           return true;
        }

        public void UnsetBits(IEnumerable<int> bitNumbers)
        {
            foreach (var bitNumber in bitNumbers)
            {
                UnsetBit(bitNumber);
            }
        }
        
        public byte[] GetBitmapBlocksByNumbers(IReadOnlyList<int> bitNumbers)
        {
            var bytes = new byte[bitNumbers.Count * _dataLength];
            
            var bitCount = _dataLength;
            var booleans = new bool[8];
            
            for (var k = 0; k < bitNumbers.Count; k++)
            {
                var bitmapBlockNumber = bitNumbers[k].DivideWithUpRounding(_dataLength * 8);
                for (int i = bitCount, j = 0; i < 2 * bitCount; i += 8, j++)
                {
                    for (var m = 0; m < 8; m++)
                        booleans[m] = _tree[bitmapBlockNumber + i + m];
                    bytes[k * bitCount + j] = booleans.ToByte();
                }
            }

            return bytes;
        }

        public bool this[int bitNumber]
        {
            get => _tree[_dataLength + bitNumber];
            set
            {
                var position = _dataLength + bitNumber;
                var divisor = 1;
                while (position >= 1)
                {
                    _tree[position] = value;
                    
                    divisor *= 2;
                    position /= divisor;
                }
            }
        }

        public int GetFirstUnsetBit()
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

        private int CountUnsetBits()
        {
            var unsetBitsCount = 0;
            for (var i = _dataLength; i < _tree.Count; i++)
                if (_tree[i])
                    unsetBitsCount++;
                    
            return unsetBitsCount;
        }

        // boolean min
        private static bool TreeOperation(bool first, bool second) => first && second;

        private void SetBit(int bitNumber)
        {
            this[bitNumber] = true;
            SetBitsCount++;
        }

        private void UnsetBit(int bitNumber)
        {
            this[bitNumber] = false;
            SetBitsCount--;
        }
    }
}