using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Heatsink Stats for any gun (heatBarSize, decaySpeed, overHeatDecaySpeed, decayCurve)
/// </summary>
[System.Serializable]
public struct HeatSinkStats
{
    [Header("Size of the heatsink")]
    public float heatSinkSize;

    [Header("Time before decaying heat and how fast")]
    public float heatDecayDelay;
    public float heatDecayPower;

    [Header("Time before decaying heat when overheated and how fast")]
    public float overheatDecayDelay;
    public float overheatDecayPower;

    [Header("Heat Decay Power Mutiplier Time input is heat percentage01")]
    public NativeSampledAnimationCurve heatDecayMultiplierCurve;


    public float GetHeatDecayMultiplier(float heatPercentage)
    {
        return heatDecayMultiplierCurve.Evaluate(heatPercentage);
    }


    /// <summary>
    /// Bake all curves from the AnimationCurves in this struct into their internal float arrays.
    /// </summary>
    public void BakeAllCurves()
    {
        heatDecayMultiplierCurve.Bake();
    }
    public void Dispose()
    {
        heatDecayMultiplierCurve.Dispose();
    }

    /// <summary>
    /// Deafault values for the heatsink.
    /// </summary>
    public static HeatSinkStats Default => new HeatSinkStats()
    {
        heatSinkSize = 1,
        heatDecayDelay = 0.25f,
        heatDecayPower = 0.1f,
        overheatDecayDelay = 1f,
        overheatDecayPower = 0.075f,
        heatDecayMultiplierCurve = NativeSampledAnimationCurve.Default,
    };
}
