using System;
using System.Threading;

namespace Jbta.VirtualFileSystem.Utils
{
    internal static class SemaphoreExtensions
    {
        public static IDisposable Lock(this SemaphoreSlim l) => new Waiter(l);

        private struct Waiter : IDisposable
        {
            private readonly SemaphoreSlim _lock;

            public Waiter(SemaphoreSlim l)
            {
                _lock = l;
                _lock.Wait();
            }

            public void Dispose() => _lock.Release();
        }
    }
}