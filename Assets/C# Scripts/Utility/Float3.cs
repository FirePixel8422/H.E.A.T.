


namespace Unity.Mathematics
{
    /// <summary>
    /// Helper struct for float3 containing constants like Up Forward and Zero
    /// </summary>
    public static class Float3
    {
        public static float3 Zero => new float3(0f, 0f, 0f);
        public static float3 One => new float3(1f, 1f, 1f);
        public static float3 Up => new float3(0f, 1f, 0f);
        public static float3 Down => new float3(0f, -1f, 0f);
        public static float3 Left => new float3(-1f, 0f, 0f);
        public static float3 Right => new float3(1f, 0f, 0f);
        public static float3 Forward => new float3(0f, 0f, 1f);
        public static float3 Backward => new float3(0f, 0f, -1f);
    }
}