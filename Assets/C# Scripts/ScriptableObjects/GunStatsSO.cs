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


    public void GetGunStats(out GunCoreStats _coreStats, out HeatSinkStats _heatSinkStats, out GunShakeStats _shakeStats)
    {
        coreStats.BakeAllCurves();
        
        _coreStats = coreStats;
        _heatSinkStats = heatSinkStats;
        _shakeStats = shakeStats;
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
