using UnityEngine;


[System.Serializable]
public class GunScopeStats : IGunAtachment
{
    public int AttachmentId { get; set; }
    public AttachmentType Type { get; set; }
    

    [SerializeField] private SmartAttributeFloat zoomMultiplier = new SmartAttributeFloat(1, ApplyMode.Override);


    public void ApplyToBaseStats(ref CompleteGunStatsSet gunStatsSet)
    {
        zoomMultiplier.ApplyToStat(ref gunStatsSet.gunADSStats.zoomMultiplier);
    }
}