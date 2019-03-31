using System.Collections.Concurrent;

namespace Jbta.VirtualFileSystem.Utils
{
    internal class ConcurrentSet<T>
    {
        private readonly object _obj;
        private readonly ConcurrentDictionary<T, object> _data;

        public ConcurrentSet()
        {
            _obj = new object();
            _data = new ConcurrentDictionary<T, object>();
        }

        public void Add(T value) => _data.GetOrAdd(value, key => _obj);

        public void Remove(T value) => _data.TryRemove(value, out _);

        public bool Contains(T value) => _data.ContainsKey(value);
    }
}