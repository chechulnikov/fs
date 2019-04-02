using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal.SpaceManagement
{
    internal class Bitmap : IDisposable
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

        public int SetBitsCount
        {
            get
            {
                using (_locker.ReaderLock())
                {
                    return _bitmapTree.SetBitsCount;
                }
            }
        }

        /// <summary>
        /// Finds first unset bit and set it
        /// </summary>
        /// <returns>First unset bit number</returns>
        public async ValueTask<int> SetFirstUnsetBit()
        {
            int firstUnsetBit;
            using (_locker.UpgradableReaderLock())
            {
                firstUnsetBit = _bitmapTree.GetFirstUnsetBit();
                using (_locker.WriterLock())
                {
                    if (_bitmapTree[firstUnsetBit])
                    {
                        firstUnsetBit = _bitmapTree.GetFirstUnsetBit();
                    }
                    
                    _bitmapTree[firstUnsetBit] = true;
                    await SaveBitmapModifications(new[] {firstUnsetBit});
                }
            }
            return firstUnsetBit;
        }

        /// <summary>
        /// Tries to set given bit
        /// </summary>
        /// <param name="bitNumber">Number of bit</param>
        /// <returns>True, if bit was successfully set</returns>
        public async Task<bool> TrySetBit(int bitNumber)
        {
            using (_locker.UpgradableReaderLock())
            {
                if (_bitmapTree[bitNumber]) return false;
                using (_locker.WriterLock())
                {
                    if (!_bitmapTree.TrySetBit(bitNumber)) return false;
                    await SaveBitmapModifications(new[] {bitNumber});
                    return true;
                }
            }
        }
        
        /// <summary>
        /// Tries to unset given bit
        /// </summary>
        /// <param name="bitNumber">Number of bit</param>
        /// <returns>True, if bit was successfully unset</returns>
        public async Task<bool> TryUnsetBit(int bitNumber)
        {
            using (_locker.UpgradableReaderLock())
            {
                if (!_bitmapTree[bitNumber]) return false;
                using (_locker.WriterLock())
                {
                    if (!_bitmapTree.TryUnsetBit(bitNumber)) return false;
                    await SaveBitmapModifications(new[] {bitNumber});
                    return true;
                }
            }
        }

        /// <summary>
        /// Unset given bits
        /// </summary>
        public async ValueTask UnsetBits(IReadOnlyList<int> bitNumbers)
        {
            using (_locker.WriterLock())
            {
                _bitmapTree.UnsetBits(bitNumbers);
                await SaveBitmapModifications(bitNumbers);
            }
        }

        private async Task SaveBitmapModifications(IReadOnlyList<int> bitNumbers)
        {
            var modifiedBitmapBlocksSnapshot = _bitmapTree.GetBitmapBlocksSnapshotsByNumbers(bitNumbers);
            await _volumeWriter.WriteBlocks(modifiedBitmapBlocksSnapshot, bitNumbers);
        }
        
        public void Dispose() => _locker?.Dispose();
    }
}