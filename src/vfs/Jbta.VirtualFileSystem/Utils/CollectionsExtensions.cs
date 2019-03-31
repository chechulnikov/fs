using System;

namespace Jbta.VirtualFileSystem.Utils
{
    internal static class CollectionsExtensions
    {
        public static byte[] ToByteArray(this int[] values)
        {
            var byteArray = new byte[values.Length * sizeof(int)];
            Buffer.BlockCopy(values, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }
    }
}