using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public struct GunSwayStats
{
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

    public float offsetSmooth;
    public float swayRecoverSmooth;

    [Header("Spread Multiplier")]
    public NativeSampledAnimationCurve spreadMultplierCurve;


    public void BakeAllCurves()
    {
        spreadMultplierCurve.Bake();
    }

    /// <summary>
    /// Returns a GunSwayStats struct with sensible defaults.
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


    public void Dispose()
    {
        spreadMultplierCurve.Dispose();
    }
}
