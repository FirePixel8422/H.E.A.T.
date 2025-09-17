using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Wrapper for AnimationCurve that allows sampling at a fixed number of points, making Evaluate() much cheaper.
/// </summary>
[BurstCompile]
[System.Serializable]
public struct NativeSampledAnimationCurve
{
    [Header("Curve HAS to go from time 0 to time 1")]
    [SerializeField] private AnimationCurve curve;

    [Header("Curve X this value is what gets baked into the curve output")]
    [SerializeField] private float valueMultiplier;

    [Header("More samples = more accurate, but more memory usage")]
    [Range(2, 500)]
    [SerializeField] private int sampleCount;

    private NativeArray<float> bakedCurve;


    /// <summary>
    /// Bake the AnimationCurve into the fixed number of samples.
    /// </summary>
    public void Bake()
    {
#if UNITY_EDITOR
        if (curve.keys.Length == 0)
        {
            DebugLogger.LogError("AnimationCurve is null!");
            return;
        }
#endif

        bakedCurve = new NativeArray<float>(sampleCount, Allocator.Persistent);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1);
            bakedCurve[i] = curve.Evaluate(t) * valueMultiplier;
        }
    }

    /// <summary>
    /// Get value based on percent (0-1) from the baked curve.
    /// </summary>
    public float Evaluate(float percent)
    {
#if UNITY_EDITOR
        if (bakedCurve == null || bakedCurve.Length != sampleCount)
        {
            DebugLogger.LogWarning("FastAnimationCurve: Baked curve is not initialized properly, call Bake before use.");
            Bake();
        }
#endif

        return EvaluateWithBurst(bakedCurve, sampleCount, percent);
    }

    /// <summary>
    /// Get value based on percent (0-1) from the baked curve, compiled with burst for super fast evaluation.
    /// </summary>
    [BurstCompile]
    private static float EvaluateWithBurst(in NativeArray<float> bakedCurve, int sampleCount, float percent)
    {
        float curvePercentage = math.clamp(percent * (sampleCount - 1), 0, sampleCount - 1);

        int floorIndex = (int)math.floor(curvePercentage);
        int ceilIndex = (int)math.ceil(curvePercentage);

        return math.lerp(bakedCurve[floorIndex], bakedCurve[ceilIndex], curvePercentage - floorIndex);
    }


    public static NativeSampledAnimationCurve Default => new NativeSampledAnimationCurve()
    {
        curve = AnimationCurve.Linear(1, 1, 0, 0),
        valueMultiplier = 1,
        sampleCount = 50,
    };


    public void Dispose()
    {
        bakedCurve.DisposeIfCreated();
    }
}
