using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public struct GunSwayStats
{
    [Header("X coord is for HIP, Y coord is for ADS data")]

    [Header("Settings")]
    public float2 movementAmplitude;
    public float movementFrequency;

    [Header("Simulates Breathing")]
    public float2 idleAmplitude;
    public float idleFrequency;

    [Header("Sway")]
    public float2 posSwayMouse;
    public float2 eulerSwayMouse;
    public float2 posSwayMove;
    public float2 eulerSwayMove;
    public bool ignoreYForSwayMove;

    public float offsetSmooth;
    public float swayRecoverSmooth;

    [Header("Gun Y Offset for scopes")]
    public float gunYOffset;

    [Header("Spread Multiplier")]
    public NativeSampledAnimationCurve spreadMultplierCurve;


    /// <summary>
    /// Bake all curves from the AnimationCurves in this struct into their internal float arrays.
    /// </summary>
    public void BakeAllCurves()
    {
        spreadMultplierCurve.Bake();
    }
    public void Dispose()
    {
        spreadMultplierCurve.Dispose();
    }

    /// <summary>
    /// Default values for SwayStats
    /// </summary>
    public static GunSwayStats Default => new GunSwayStats
    {
        movementAmplitude = new float2(0.05f, 0.05f),
        movementFrequency = 6f,
        idleAmplitude = new float2(0.025f, 0.025f),
        idleFrequency = 0.5f,
        posSwayMouse = new float2(0.005f, 0.005f),
        eulerSwayMouse = new float2(0.001f, 0.001f),
        posSwayMove = new float2(0.005f, 0.005f),
        eulerSwayMove = new float2(0.001f, 0.001f),
        offsetSmooth = 12f,
        swayRecoverSmooth = 12f,
        spreadMultplierCurve = NativeSampledAnimationCurve.Default
    };

}
