using UnityEngine;
using FirePixel.Networking;


public class GunManager : MonoBehaviour
{
    public static GunManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }



    [SerializeField] public GunStatsSO[] guns;
    private int currentGunId;

    /// <summary>
    /// Get gun stats by gunId.
    /// </summary>
    public void GetGunStats(int gunId, out GunCoreStats coreStats, out HeatSinkStats heatSinkStats)
    {
        currentGunId = gunId;

        GunStatsSO targetGun = guns[gunId];

        coreStats = targetGun.GetCoreStats();
        heatSinkStats = targetGun.GetHeatSinkStats();
    }

    public void GetNextGunStats(out GunCoreStats coreStats, out HeatSinkStats heatSinkStats, out int gunId)
    {
        currentGunId = (currentGunId + 1) % guns.Length;
        gunId = currentGunId;

        GunStatsSO targetGun = guns[gunId];

        coreStats = targetGun.GetCoreStats();
        heatSinkStats = targetGun.GetHeatSinkStats();
    }
}
