


[System.Serializable]
public struct CompleteGunStatsSet
{
    public GunCoreStats coreStats;
    public GunAudioStats audioStats;
    public GunShakeStats shakeStats;
    public GunADSStats gunADSStats;
    public GunSwayStats swayStats;
    public HeatSinkStats heatSinkStats;


    /// <summary>
    /// copy all data into input parameter structs
    /// </summary>
    public void GetStatsCopy(out GunCoreStats coreStats, out GunAudioStats audioStats, out HeatSinkStats heatSinkStats, out GunShakeStats shakeStats, out GunSwayStats swayStats, out GunADSStats gunADSStats)
    {
        coreStats = this.coreStats;
        audioStats = this.audioStats;
        swayStats = this.swayStats;
        heatSinkStats = this.heatSinkStats;
        shakeStats = this.shakeStats;
        gunADSStats = this.gunADSStats;
    }

    public void BakeAllCurves()
    {
        coreStats.BakeAllCurves();
        swayStats.BakeAllCurves();
        heatSinkStats.BakeAllCurves();
    }

    public void Dispose()
    {
        coreStats.Dispose();
        swayStats.Dispose();
        heatSinkStats.Dispose();
    }


    public static CompleteGunStatsSet Default => new CompleteGunStatsSet
    {
        coreStats = GunCoreStats.Default,
        shakeStats = GunShakeStats.Default,
        gunADSStats = GunADSStats.Default,
        swayStats = GunSwayStats.Default,
        heatSinkStats = HeatSinkStats.Default
    };
}