using UnityEngine;



[System.Serializable]
public struct SmartAttributeMinMaxFloat
{
    [Header("Value to use")]
    [SerializeField] private MinMaxFloat value;

    [Header("How to apply the value")]
    [SerializeField] private ApplyMode applymode;


    public void ApplyToStat(ref MinMaxFloat stat)
    {
        stat = applymode switch
        {
            ApplyMode.Add => stat + value,
            ApplyMode.Multiply => stat * value,
            ApplyMode.Override => value,
            _ => default,
        };
    }

    public SmartAttributeMinMaxFloat(MinMaxFloat value, ApplyMode applymode)
    {
        this.value = value;
        this.applymode = applymode;
    }
}