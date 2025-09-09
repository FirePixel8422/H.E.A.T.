


namespace Unity.Mathematics
{
    /// <summary>
    /// Helper struct or int3 containing constants like Up Forward and Zero
    /// </summary>
    public struct Int3
    {
        public static int3 Zero => new int3(0, 0, 0);
        public static int3 One => new int3(1, 1, 1);
        public static int3 Up => new int3(0, 1, 0);
        public static int3 Down => new int3(0, -1, 0);
        public static int3 Left => new int3(-1, 0, 0);
        public static int3 Right => new int3(1, 0, 0);
        public static int3 Forward => new int3(0, 0, 1);
        public static int3 Backward => new int3(0, 0, -1);
    }
}