using Unity.Mathematics;
using UnityEngine;



[CreateAssetMenu(fileName = "GunCoreStatsSO", menuName = "ScriptableObjects/GunData")]
public class GunStatsSO : ScriptableObject
{
    [SerializeField] private GunCoreStats coreStats = GunCoreStats.Default;

    [Header("This AnimatioCurve is not in stats data to spare memory usage\nAt how much heat (0-1) do you have how much spread")]
    [SerializeField] private AnimationCurve spreadCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private HeatSinkStats heatSinkStats = HeatSinkStats.Default;


    public GunCoreStats GetCoreStats()
    {
        UpdateSpreadCurve(ref coreStats);

        return coreStats;
    }

    public void UpdateSpreadCurve(ref GunCoreStats stats)
    {
        int spreadCurveSampleCount = stats.spreadCurveSampleCount;

        stats.bakedSpreadCurve = new float[spreadCurveSampleCount];

        for (int i = 0; i < spreadCurveSampleCount; i++)
        {
            float t = (float)i / (spreadCurveSampleCount - 1);
            stats.bakedSpreadCurve[i] = stats.maxSpread * spreadCurve.Evaluate(t);
        }
    }

    public HeatSinkStats GetHeatSinkStats()
    {
        return heatSinkStats;
    }
}
