namespace Jbta.VirtualFileSystem.Utils
{
    internal static class MeasureUnits
    {
        public static ulong KiB(this int value) => (ulong) value * 1024;
        public static ulong MiB(this int value) => (ulong) value * 1024 * 1024;
        public static ulong GiB(this int value) => (ulong) value * 1024 * 1024 * 1024;
    }
}