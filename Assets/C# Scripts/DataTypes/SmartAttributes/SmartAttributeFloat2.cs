using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public struct SmartAttributeFloat2
{
    [Header("Value to use")]
    [SerializeField] private float2 value;

    [Header("How to apply the value")]
    [SerializeField] private ApplyMode applymode;


    public void ApplyToStat(ref float2 stat)
    {
        stat = applymode switch
        {
            ApplyMode.Add => stat + value,
            ApplyMode.Multiply => stat * value,
            ApplyMode.Override => value,
            _ => default,
        };
    }

    public SmartAttributeFloat2(float2 value, ApplyMode applymode)
    {
        this.value = value;
        this.applymode = applymode;
    }
}