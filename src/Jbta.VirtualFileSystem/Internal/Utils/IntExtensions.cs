namespace Jbta.VirtualFileSystem.Internal.Utils
{
    internal static class IntExtensions
    {
        public static int DivideWithUpRounding(this int value, int divisor)
        {
            var div = value / divisor;
            return value % divisor == 0 ? div : div + 1;
        }
    }
}