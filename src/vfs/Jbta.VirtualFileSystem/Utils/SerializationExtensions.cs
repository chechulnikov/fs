using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Vfs.Utils
{
    internal static class SerializationExtensions
    {
        public static Span<byte> Serialize(this object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        
        public static T Deserialize<T>(this Span<byte> data)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(data.ToArray()))
            {
                return (T) bf.Deserialize(ms);
            }
        }
    }
}