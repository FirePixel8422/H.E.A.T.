using Unity.Mathematics;
using UnityEngine;



[CreateAssetMenu(fileName = "GunCoreStatsSO", menuName = "ScriptableObjects/GunData")]
public class GunStatsSO : ScriptableObject
{
    [SerializeField] private GunCoreStats coreStats = GunCoreStats.Default;
    [SerializeField] private HeatSinkStats heatSinkStats = HeatSinkStats.Default;


    public GunCoreStats GetCoreStats()
    {
        coreStats.BakeAllCurves();

        return coreStats;
    }

    public HeatSinkStats GetHeatSinkStats()
    {
        return heatSinkStats;
    }

#if UNITY_EDITOR
    public void SetGunStatsADSRecoilPattern(float2[] pattern, float patternTime)
    {
        coreStats.adsRecoilPattern = pattern;
        coreStats.shootIntensityGainMultplier = patternTime;
        coreStats.shootIntensityDescreaseMultplier = patternTime * 5;
    }
#endif
}
