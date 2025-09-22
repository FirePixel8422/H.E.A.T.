


[System.Serializable]
public struct GunSwayStats
{        
    public NativeSampledAnimationCurve spreadMultplierCurve;


    /// <summary>
    /// Bake all curves from the AnimationCurve to the internal float array.
    /// </summary>
    public void BakeAllCurves()
    {
        spreadMultplierCurve.Bake();
    }

    /// <summary>
    /// Free all allocated native mmemory underlying in this struct
    /// </summary>
    public void Dispose()
    {
        spreadMultplierCurve.Dispose();
    }

    public static GunSwayStats Default => new GunSwayStats()
    {
        spreadMultplierCurve = NativeSampledAnimationCurve.Default
    };
}