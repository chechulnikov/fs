using Vfs.Utils;

namespace Vfs
{
    internal static class Default
    {
        public static int BlockSize = (int) 1.KiB();
        public static int BlocksCountPerAllocationGroup = 8197;
    }
}