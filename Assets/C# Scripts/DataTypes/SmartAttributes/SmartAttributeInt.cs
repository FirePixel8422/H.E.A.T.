using UnityEngine;



[System.Serializable]
public struct SmartAttributeInt
{
    [SerializeField] private int value;
    [SerializeField] private ApplyMode applyMode;


    public void ApplyToStat(ref int stat)
    {
        stat = applyMode switch
        {
            ApplyMode.Add => stat + value,
            ApplyMode.Multiply => stat * value,
            ApplyMode.Override => value,
            _ => stat,
        };
    }

    public SmartAttributeInt(int value, ApplyMode applyMode)
    {
        this.value = value;
        this.applyMode = applyMode;
    }
}