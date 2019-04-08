using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jbta.VirtualFileSystem.Exceptions;
using Jbta.VirtualFileSystem.Internal.Utils;

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
        private readonly int _blockSize;
        private readonly int _dataLength;
        private readonly BitArray _tree;

        public BitmapTree(int blockSize, byte[] data)
        {
            _blockSize = blockSize;
            var bitmapData = new BitArray(data);
            
            _dataLength = bitmapData.Length;
            _tree = new BitArray(2 * bitmapData.Length);
            
            for (var i = 0; i < bitmapData.Length; i++)
                _tree[bitmapData.Length + i] = bitmapData[i];
            
            for (var i = bitmapData.Length - 1; i >= 0; i--)
                _tree[i] = TreeOperation(_tree[2 * i], _tree[2 * i + 1]);
            
            // count set bits
            for (var i = 0; i < _dataLength; i++)
                if (this[i]) SetBitsCount++;
        }

        public int SetBitsCount { get; private set; }

        public int SetBitsCount2
        {
            get
            {
                var res = 0;
                for (var i = 0; i < _dataLength; i++)
                    if (this[i]) res++;
                return res;
            }
        }

        public bool TrySetBit(int bitNumber)
        {
           if (this[bitNumber]) return false;
           SetBit(bitNumber);
           return true;
        }
        
        public bool TryUnsetBit(int bitNumber)
        {
            if (!this[bitNumber]) return false;
            UnsetBit(bitNumber);
            return true;
        }

        public void UnsetBits(IEnumerable<int> bitNumbers)
        {
            foreach (var bitNumber in bitNumbers)
            {
                UnsetBit(bitNumber);
            }
        }

        public byte[] GetBitmapBlocksSnapshotsByNumbers(IEnumerable<int> bitNumbers)
        {
            var bitsInBlock = _blockSize * 8;
            var booleansBuffer = new bool[8];

            var bitmapBlocksNumbers = bitNumbers
                .Select(bm => bm < bitsInBlock ? 0 : bm.DivideWithUpRounding(bitsInBlock))
                .ToArray();
            
            var result = new byte[bitmapBlocksNumbers.Length * _blockSize];
            
            for (var k = 0; k < bitmapBlocksNumbers.Length; k++)
            {
                var bitmapBlockNumber = bitmapBlocksNumbers[k];
                
                // extract bitmap block byte by byte
                for (int i = bitsInBlock, j = 0; i < 2 * bitsInBlock; i += 8, j++)
                {
                    // 8 booleans values equals 1 byte
                    for (var m = 0; m < 8; m++)
                    {
                        booleansBuffer[m] = _tree[bitmapBlockNumber + i + m];
                    }
                    
                    result[k * _blockSize + j] = booleansBuffer.ToByte();
                }
            }

            return result;
        }

        public bool this[int bitNumber]
        {
            get => _tree[_dataLength + bitNumber];
            set
            {
                var position = _dataLength + bitNumber;
                _tree[position] = value;
                
                while (position >= 2)
                {
                    var t = position % 2 == 0
                        ? TreeOperation(_tree[position], _tree[position + 1])
                        : TreeOperation(_tree[position - 1], _tree[position]);

                    position /= 2;

                    if (_tree[position] == t)
                    {
                        break;
                    }

                    _tree[position] = t;
                }
            }
        }

        public int GetFirstUnsetBit()
        {
            var position = 1;
            while (position < _dataLength)
            {
                position *= 2;

                var left = _tree[position];
                var right = _tree[position + 1];

                if (!left) continue;
                if (right) throw new FileSystemException("Invalid bitmap tree state");

                position += 1;
            }
                
            return position - _dataLength;
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