using System;
using System.Threading;

namespace Jbta.VirtualFileSystem.Utils
{
    internal static class ReaderWriterLockSlimExtensions
    {
        public static IDisposable ReaderLock(this ReaderWriterLockSlim l) => new ReadLock(l);

        public static IDisposable UpgradableReaderLock(this ReaderWriterLockSlim l) => new UpgradableReadLock(l);

        public static IDisposable WriterLock(this ReaderWriterLockSlim l) => new WriteLock(l);

        private struct ReadLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public ReadLock(ReaderWriterLockSlim l)
            {
                _lock = l;
                _lock.EnterReadLock();
            }

            public void Dispose()
            {
                _lock.ExitReadLock();
            }
        }

        private struct UpgradableReadLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public UpgradableReadLock(ReaderWriterLockSlim l)
            {
                _lock = l;
                _lock.EnterUpgradeableReadLock();
            }

            public void Dispose()
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        private struct WriteLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public WriteLock(ReaderWriterLockSlim l)
            {
                _lock = l;
                _lock.EnterWriteLock();
            }

            public void Dispose()
            {
                _lock.ExitWriteLock();
            }
        }
    }
}