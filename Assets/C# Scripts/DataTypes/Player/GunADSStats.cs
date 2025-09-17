using UnityEngine;



[System.Serializable]
public struct GunADSStats
{
    [Header("X1, X2, X4, etc")]
    public float zoomMultiplier;

    [Header("Time to go from hipFire view to ADS view")]
    public float zoomInTime;

    [Header("Time to go from ADS view back to hipFire view")]
    public float zoomOutTime;

    [Header("Name of the ads zoom animation")]
    public string normalAnimationName;
    public string zoomAnimationName;
    public int animLayer;

    public static GunADSStats Default => new GunADSStats()
    {
        zoomMultiplier = 1.0f,

        zoomInTime = 0.5f,
        zoomOutTime = 0.25f
    };
}