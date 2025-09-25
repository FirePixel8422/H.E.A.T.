using UnityEngine;



[System.Serializable]
public struct SmartAttributeInt
{
    [Header("Value to use")]
    [SerializeField] private int value;

    [Header("How to apply the value")]
    [SerializeField] private ApplyMode applymode;


    public void ApplyToStat(ref int stat)
    {
        stat = applymode switch
        {
            ApplyMode.Add => stat + value,
            ApplyMode.Multiply => stat * value,
            ApplyMode.Override => value,
            _ => default,
        };
    }

    public SmartAttributeInt(int value, ApplyMode applymode)
    {
        this.value = value;
        this.applymode = applymode;
    }
}