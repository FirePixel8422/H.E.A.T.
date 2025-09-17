using Unity.Mathematics;
using UnityEngine;



[CreateAssetMenu(fileName = "GunCoreStatsSO", menuName = "ScriptableObjects/GunData")]
public class GunStatsSO : ScriptableObject
{
    [Header("Actual gun")]
    [SerializeField] private GunRefHolder gunPrefab;
    public GunRefHolder GunPrefab => gunPrefab;

    [Header("Stats bound to this gun")]
    [SerializeField] private GunCoreStats coreStats = GunCoreStats.Default;
    [SerializeField] private HeatSinkStats heatSinkStats = HeatSinkStats.Default;
    [SerializeField] private GunShakeStats shakeStats = GunShakeStats.Default;
    [SerializeField] private GunSwayStats swayStats = GunSwayStats.Default;


    public void GetGunStats(out GunCoreStats coreStats, out HeatSinkStats heatSinkStats, out GunShakeStats shakeStats, out GunSwayStats swayStats)
    {
        this.coreStats.BakeAllCurves();
        
        coreStats = this.coreStats;
        heatSinkStats = this.heatSinkStats;
        shakeStats = this.shakeStats;
        swayStats = this.swayStats;
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
