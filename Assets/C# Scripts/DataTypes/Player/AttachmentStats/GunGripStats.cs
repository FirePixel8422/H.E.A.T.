using UnityEngine;


[System.Serializable]
public class GunGripStats : IGunAtachment
{
    public int AttachmentId { get; set; }
    public AttachmentType Type { get; set; }


    [SerializeField] private SmartAttributeFloat zoomMultiplier;


    public void ApplyToBaseStats(ref CompleteGunStatsSet gunStatsSet)
    {
        zoomMultiplier.ApplyToStat(ref gunStatsSet.gunADSStats.zoomMultiplier);
    }

    public void ApplyToGunObject(GunRefHolder gunRef)
    {
        return;
    }
}