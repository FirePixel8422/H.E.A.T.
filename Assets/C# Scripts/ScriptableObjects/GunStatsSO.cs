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
}
