using UnityEngine;



[System.Serializable]
public struct SmartAttributeFloat
{
    [Header("Value to use")]
    [SerializeField] private float value;

    [Header("How to apply the value")]
    [SerializeField] private ApplyMode applymode;


    public void ApplyToStat(ref float stat)
    {
        stat = applymode switch
        {
            ApplyMode.Add => stat + value,
            ApplyMode.Multiply => stat * value,
            ApplyMode.Override => value,
            _ => default,
        };
    }

    public SmartAttributeFloat(float value, ApplyMode applymode)
    {
        this.value = value;
        this.applymode = applymode;
    }
}