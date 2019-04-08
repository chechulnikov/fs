using System;
using System.Linq;

namespace Jbta.VirtualFileSystem.Internal.Utils
{
    internal static class RandomString
    {
        private static readonly Random Random = new Random();
        
        public static string Generate(int length)
        {
            const string chars = "abcedefghijklmnopqrstuvwzyxABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}