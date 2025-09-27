using UnityEngine;


[System.Serializable]
public class GunMuzzleStats : IGunAtachment
{
    public int AttachmentId { get; set; }

    [SerializeField] private SmartAttributeFloat zoomMultiplier;


    public void ApplyToBaseStats(ref CompleteGunStatsSet gunStatsSet)
    {
        zoomMultiplier.ApplyToStat(ref gunStatsSet.gunADSStats.zoomMultiplier);
    }
}