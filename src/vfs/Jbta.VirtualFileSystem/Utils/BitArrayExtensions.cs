using System.Collections;

namespace Jbta.VirtualFileSystem.Utils
{
    internal static class BitArrayExtensions
    {
        public static byte[] ToByteArray(this BitArray bitArray)
        {
            var bytesCount = bitArray.Length % 8 == 0 ? bitArray.Length / 8 : bitArray.Length / 8 + 1;
            var bytes = new byte[bytesCount];
            bitArray.CopyTo(bytes, 0);
            return bytes;
        }
    }
}