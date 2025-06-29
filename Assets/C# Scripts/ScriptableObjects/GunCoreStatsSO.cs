using Unity.Mathematics;
using UnityEngine;



[CreateAssetMenu(fileName = "GunCoreStatsSO", menuName = "ScriptableObjects/GunCore Stats")]
public class GunCoreStatsSO : ScriptableObject
{
    [SerializeField] private GunCoreStats stats = GunCoreStats.Default;

    [Header("This AnimatioCurve is not in stats data to spare memory usage\nAt how much heat (0-1) do you have how much spread")]
    [SerializeField] private AnimationCurve spreadCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public GunCoreStats GetStats()
    {
        UpdateSpreadCurve(ref stats);

        return stats;
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
}
