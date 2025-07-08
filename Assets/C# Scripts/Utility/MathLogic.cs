using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;



[BurstCompile(DisableSafetyChecks = true)]
public static class MathLogic
{
    public static float DistanceFrom(this float3 value, float3 toSubtract)
    {
        float3 difference = value - toSubtract;
        return math.abs(difference.x) + math.abs(difference.y) + math.abs(difference.z);
    }

    public static int DistanceFrom(this int3 value, int3 toSubtract)
    {
        int3 difference = value - toSubtract;
        return math.abs(difference.x) + math.abs(difference.y) + math.abs(difference.z);
    }

    /// <returns>Absolute of: X + Y + Z</returns>
    public static float AbsoluteSum(this float3 value)
    {
        return math.abs(value.x) + math.abs(value.y) + math.abs(value.z);
    }

    /// <summary>
    /// Clamp float to 0 or more
    /// </summary>
    public static float ClampMin0(float value)
    {
        return 0 > value ? 0 : value;
    }


    [BurstCompile(DisableSafetyChecks = true)]
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        float delta = target - current;
        if (math.abs(delta) <= maxDelta) return target;
        return current + math.sign(delta) * maxDelta;
    }

    [BurstCompile(DisableSafetyChecks = true)]
    public static int ConvertToPowerOf2(int input)
    {
        if (input == 0) return 0;
        if (input == 1) return 1;
        if (input == 2) return 2;
        return 1 << (input - 1); // 2^(input-1)
    }

    /// <summary>
    /// Convert AnimationCurve to float array with a certain sample count.
    /// </summary>
    public static float[] BakeCurveToArray(AnimationCurve curve, int sampleCount)
    {
        if (sampleCount <= 1)
            sampleCount = 2;

        float[] values = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1);
            values[i] = curve.Evaluate(t);
        }

        return values;
    }

    /// <summary>
    /// Convert AnimationCurve to float array with a certain sample count. Multiply each entry by the given multiplier.
    /// </summary>
    public static float[] BakeCurveToArray(AnimationCurve curve, int sampleCount, float perEntryMultiplier)
    {
        if (sampleCount <= 1)
            sampleCount = 2;

        float[] values = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1);
            values[i] = curve.Evaluate(t) * perEntryMultiplier;
        }

        return values;
    }
}