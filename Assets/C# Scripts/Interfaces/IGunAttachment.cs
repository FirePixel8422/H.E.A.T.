



public interface IGunAtachment
{
    public int AttachmentId { get; set; }
    public AttachmentType Type { get; set; }

    public void ApplyToBaseStats(ref CompleteGunStatsSet gunStatsSet);
}