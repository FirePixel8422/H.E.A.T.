using UnityEngine;



[System.Serializable]
public struct SmartAttributeFloat
{
    [SerializeField] private float value;
    [SerializeField] private ApplyMode applyMode;


    public void ApplyToStat(ref float stat)
    {
        stat = applyMode switch
        {
            ApplyMode.Add => stat + value,
            ApplyMode.Multiply => stat * value,
            ApplyMode.Override => value,
            _ => stat,
        };
    }

    public SmartAttributeFloat(float value, ApplyMode applyMode)
    {
        this.value = value;
        this.applyMode = applyMode;
    }
}