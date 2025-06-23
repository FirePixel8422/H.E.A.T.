using UnityEngine;



[CreateAssetMenu(fileName = "HeatSinkStats", menuName = "ScriptableObjects/HeatSink Stats")]
public class HeatSinkStatsSO : ScriptableObject
{
    public HeatSinkStats stats = HeatSinkStats.Default;
}
