using UnityEngine;



[System.Serializable]
public struct GunSwayStats
{

    [Header("ADS")]
    [Tooltip("Multiplier applied when zoomed in (lerped via ZoomPercentage). < 1 reduces sway.")]
    public float adsMultiplier;

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

    };

    public void Dispose()
    {
        spreadMultplierCurve.Dispose();
    }
}
