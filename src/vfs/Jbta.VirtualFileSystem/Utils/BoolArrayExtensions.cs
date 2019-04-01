using System.Collections.Generic;

namespace Jbta.VirtualFileSystem.Utils
{
    internal static class BoolArrayExtensions
    {
        public static byte ToByte(this IEnumerable<bool> arr)
        {
            byte value = 0;
            foreach (var b in arr)
            {
                value <<= 1;
                if (b) value |= 1;
            }
            return value;
        }
    }
}