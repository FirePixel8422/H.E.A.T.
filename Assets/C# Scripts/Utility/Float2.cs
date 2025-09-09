


namespace Unity.Mathematics
{
    /// <summary>
    /// Helper struct for float2 containing constants like Up Forward and Zero
    /// </summary>
    public static class Float2
    {
        public static float2 Zero => new float2(0f, 0f);
        public static float2 One => new float2(1f, 1f);
        public static float2 Left => new float2(-1f, 0f);
        public static float2 Right => new float2(1f, 0f);
        public static float2 Up => new float2(0f, 1f);
        public static float2 Down => new float2(0f, -1f);
    }
}