using UnityEngine;



[CreateAssetMenu(fileName = "GunCoreStatsSO", menuName = "ScriptableObjects/GunData")]
public class GunStatsSO : ScriptableObject
{
    [SerializeField] private GunCoreStats coreStats = GunCoreStats.Default;

    [Header("This AnimatioCurve is not in stats data to spare memory usage\nAt how much % damage dealt at how many % of coreStats.maxEffectiveRange")]
    [SerializeField] private AnimationCurve damageFallOffCurve = AnimationCurve.Linear(1, 1, 0, 0);

    [Header("This AnimatioCurve is not in stats data to spare memory usage\nAt how much heat (0-1) do you have how much spread")]
    [SerializeField] private AnimationCurve spreadCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private HeatSinkStats heatSinkStats = HeatSinkStats.Default;


    public GunCoreStats GetCoreStats()
    {
        UpdateDamageFallOffCurve(ref coreStats);
        UpdateSpreadCurve(ref coreStats);

        return coreStats;
    }

    public void UpdateDamageFallOffCurve(ref GunCoreStats stats)
    {
        int falloffSampleCount = stats.damageFallOffSampleCount;

        stats.bakedDamageCurve = MathLogic.BakeCurveToArray(damageFallOffCurve, falloffSampleCount, stats.damage);
    }
    public void UpdateSpreadCurve(ref GunCoreStats stats)
    {
        int spreadSampleCount = stats.spreadSampleCount;

        stats.bakedSpreadCurve = MathLogic.BakeCurveToArray(spreadCurve, spreadSampleCount, stats.maxSpread);
    }

    public HeatSinkStats GetHeatSinkStats()
    {
        return heatSinkStats;
    }
}
