using System;

namespace Jbta.VirtualFileSystem.Utils
{
    internal static class ByteArrayExtensions
    {
        public static int[] ToIntArray(this byte[] byteArray)
        {
            var count = byteArray.Length.DivideWithUpRounding(sizeof(int));
            var result = new int[count];
            
            for (int i = 0, offset = 0; i < count; i++, offset += sizeof(int))
            {
                result[i] = BitConverter.ToInt32(byteArray, offset);
            }
            
            return result;
        }
    }
}