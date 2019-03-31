using System;
using System.Collections;
using System.Threading;
using Vfs.Utils;

namespace Vfs
{
    /// <summary>
    /// This tree provides first unset bit search search for O(log n)
    /// Example:
    ///    0       0
    ///  1   0   0   0
    /// 1 1 1 0 1 0 0 1
    /// </summary>
    internal class BitmapTree : IDisposable
    {
        private readonly ReaderWriterLockSlim _locker;
        private readonly int _dataLength;
        private readonly BitArray _tree;
        
        public BitmapTree(byte[] data)
        {
            _locker = new ReaderWriterLockSlim();
            
            var bitmapData = new BitArray(data);
            
            _dataLength = bitmapData.Length;
            _tree = new BitArray(2 * bitmapData.Length);
            
            for (var i = 0; i < bitmapData.Length; i++)
                _tree[bitmapData.Length + i] = bitmapData[i];
            
            for (var i = bitmapData.Length - 1; i >= 0; i--)
                _tree[i] = TreeOperation(_tree[2 * i], _tree[2 * i + 1]);
        }
        
        /// <summary>
        /// Finds first unset bit and set it
        /// </summary>
        /// <returns>First unset bit number</returns>
        public int SetFirstUnsetBit()
        {
            int firstUnsetBit;
            
            using (_locker.UpgradableReaderLock())
            {
                firstUnsetBit = GetFirstUnsetBit();
                using (_locker.WriterLock())
                {
                    if (this[firstUnsetBit]) firstUnsetBit = GetFirstUnsetBit();
                    
                    this[firstUnsetBit] = true;
                }
            }

            return firstUnsetBit;
        }

        /// <summary>
        /// Tries to set given bit
        /// </summary>
        /// <param name="bitNumber">Number of bit</param>
        /// <returns>True, if bit was successfully set</returns>
        public bool TrySetBit(int bitNumber)
        {
            if (this[bitNumber]) return false;
            using (_locker.WriterLock())
            {
               if (this[bitNumber]) return false;
               this[bitNumber] = true;
               return true;
            }
        }

        public void Dispose() => _locker?.Dispose();
        
        // boolean min
        private static bool TreeOperation(bool first, bool second) => first && second;

        private int GetFirstUnsetBit()
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

        private bool this[int bitNumber]
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
    }
}