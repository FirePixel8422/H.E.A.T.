using UnityEngine;


[System.Serializable]
public struct GunADSStats
{
    [Header("X1, X2, X4, etc")]
    public float zoomMultiplier;
    [Header("For sniper scope")]
    public float scopeCamZoomMultiplier;

    [Header("Sensitivity multiplier while zoomed in (No Live Update)")]
    public float adsSensitivityMultiplier;

    [Header("Time to go from hipFire view to ADS view")]
    public float zoomInTime;
    [Header("Time to go from ADS view back to hipFire view")]
    public float zoomOutTime;

    [Header("Names of the normal and ads zoom animations")]
    public string normalAnimationName;
    public string zoomAnimationName;
    public int animLayer;

    public void GetAnimationHashes(out int normalAnimationHash, out int zoomAnimationHash)
    {
        normalAnimationHash = Animator.StringToHash(normalAnimationName);
        zoomAnimationHash = Animator.StringToHash(zoomAnimationName);
    }

    public static GunADSStats Default => new GunADSStats()
    {
        zoomMultiplier = 1.0f,
        scopeCamZoomMultiplier = 1,
        adsSensitivityMultiplier = 1.0f,

        zoomInTime = 0.5f,
        zoomOutTime = 0.25f,

        normalAnimationName = "ADS_Normal",
        zoomAnimationName = "ADS_Zoom",
        animLayer = 2,
    };
}