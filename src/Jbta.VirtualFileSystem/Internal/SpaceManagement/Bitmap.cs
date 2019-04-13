using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Internal.Utils;
using Nito.AsyncEx;

namespace Jbta.VirtualFileSystem.Internal.SpaceManagement
{
    internal class Bitmap
    {
        private readonly BitmapTree _bitmapTree;
        private readonly IVolumeWriter _volumeWriter;
        private readonly ReaderWriterLockSlim _locker;

        public Bitmap(BitmapTree bitmapTree, IVolumeWriter volumeWriter)
        {
            _bitmapTree = bitmapTree;
            _volumeWriter = volumeWriter;
            _locker = new ReaderWriterLockSlim();
        }

        public int MarkedBitsCount
        {
            get
            {
                using (_locker.ReaderLock())
                {
                    return _bitmapTree.MarkedBitsCount;
                }
            }
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
                firstUnsetBit = _bitmapTree.GetFirstUnsetBit();
                using (_locker.WriterLock())
                {
                    _bitmapTree[firstUnsetBit] = true;
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
            using (_locker.UpgradableReaderLock())
            {
                if (_bitmapTree[bitNumber]) return false;
                using (_locker.WriterLock())
                {
                    return _bitmapTree.TrySetBit(bitNumber);
                }
            }
        }
        
        /// <summary>
        /// Tries to unset given bit
        /// </summary>
        /// <param name="bitNumber">Number of bit</param>
        /// <returns>True, if bit was successfully unset</returns>
        public bool TryUnsetBit(int bitNumber)
        {
            using (_locker.UpgradableReaderLock())
            {
                if (!_bitmapTree[bitNumber]) return false;
                using (_locker.WriterLock())
                {
                    return _bitmapTree.TryUnsetBit(bitNumber);
                }
            }
        }

        /// <summary>
        /// Unset given bits
        /// </summary>
        public void UnsetBits(IEnumerable<int> bitNumbers)
        {
            using (_locker.WriterLock())
            {
                _bitmapTree.UnsetBits(bitNumbers);
            }
        }

        public async Task SaveBitmapModifications(IReadOnlyList<int> bitNumbers)
        {
            byte[] modifiedBitmapBlocksSnapshot;
            using (_locker.ReaderLock())
            {
                modifiedBitmapBlocksSnapshot = _bitmapTree.GetPartialBitmapSnapshot(bitNumbers);
            }

            await _volumeWriter.WriteBlocks(modifiedBitmapBlocksSnapshot, bitNumbers);
            
        }
    }
}