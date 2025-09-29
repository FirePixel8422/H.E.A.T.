using UnityEngine;


[System.Serializable]
public class GunMuzzleStats : IGunAtachment
{
    public int AttachmentId { get; set; }

    [Header("List of audio clips to randomly pick and play when shooting")]
    [SerializeField] private AudioClip[] shootAudioClips;
    [SerializeField] private SmartAttributeMinMaxFloat minMaxPitch = new SmartAttributeMinMaxFloat(new MinMaxFloat(1, 1), ApplyMode.Override);
    [SerializeField] private SmartAttributeMinMaxFloat minMaxPitchAtMaxHeat = new SmartAttributeMinMaxFloat(new MinMaxFloat(1, 1), ApplyMode.Override);


    public void ApplyToBaseStats(ref CompleteGunStatsSet gunStatsSet)
    {
        gunStatsSet.coreStats.shootAudioClips = shootAudioClips;

        minMaxPitch.ApplyToStat(ref gunStatsSet.coreStats.minMaxPitch);
        minMaxPitchAtMaxHeat.ApplyToStat(ref gunStatsSet.coreStats.minMaxPitchAtMaxHeat);
    }
}