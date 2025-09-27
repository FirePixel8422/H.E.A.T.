using UnityEngine;



[System.Serializable]
public struct SmartAttributeMinMaxFloat
{
    [Header("Value to use")]
    [SerializeField] private MinMaxFloat value;

    [Header("How to apply the value")]
    [SerializeField] private ApplyMode applyMode;


    public void ApplyToStat(ref MinMaxFloat stat)
    {
        stat = applyMode switch
        {
            ApplyMode.Add => stat + value,
            ApplyMode.Multiply => stat * value,
            ApplyMode.Override => value,
            _ => stat,
        };
    }

    public SmartAttributeMinMaxFloat(MinMaxFloat value, ApplyMode applyMode)
    {
        this.value = value;
        this.applyMode = applyMode;
    }
}