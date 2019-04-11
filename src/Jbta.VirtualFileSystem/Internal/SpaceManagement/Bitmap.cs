using System.Collections.Generic;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess;
using Nito.AsyncEx;

namespace Jbta.VirtualFileSystem.Internal.SpaceManagement
{
    internal class Bitmap
    {
        private readonly BitmapTree _bitmapTree;
        private readonly IVolumeWriter _volumeWriter;
        private readonly AsyncReaderWriterLock _locker;

        public Bitmap(BitmapTree bitmapTree, IVolumeWriter volumeWriter)
        {
            _bitmapTree = bitmapTree;
            _volumeWriter = volumeWriter;
            _locker = new AsyncReaderWriterLock();
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
            using (await _locker.WriterLockAsync())
            {
                firstUnsetBit = _bitmapTree.GetFirstUnsetBit();
                
                if (_bitmapTree[firstUnsetBit])
                {
                    firstUnsetBit = _bitmapTree.GetFirstUnsetBit();
                }
                
                _bitmapTree[firstUnsetBit] = true;
                await SaveBitmapModifications(new[] {firstUnsetBit});
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
            using (await _locker.WriterLockAsync())
            {
                if (_bitmapTree[bitNumber]) return false;
                
                if (!_bitmapTree.TrySetBit(bitNumber)) return false;
                await SaveBitmapModifications(new[] {bitNumber});
                return true;
            }
        }
        
        /// <summary>
        /// Tries to unset given bit
        /// </summary>
        /// <param name="bitNumber">Number of bit</param>
        /// <returns>True, if bit was successfully unset</returns>
        public async Task<bool> TryUnsetBit(int bitNumber)
        {
            using (await _locker.WriterLockAsync())
            {
                if (!_bitmapTree[bitNumber]) return false;
                
                if (!_bitmapTree.TryUnsetBit(bitNumber)) return false;
                await SaveBitmapModifications(new[] {bitNumber});
                return true;
            }
        }

        /// <summary>
        /// Unset given bits
        /// </summary>
        public async ValueTask UnsetBits(IReadOnlyList<int> bitNumbers)
        {
            using (await _locker.WriterLockAsync())
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
    }
}