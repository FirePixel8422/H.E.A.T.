using UnityEngine;
using FirePixel.Networking;


public class GunManager : MonoBehaviour
{
    public static GunManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }



    [SerializeField] private GunStatsSO[] guns;

    /// <summary>
    /// Get gun stats by gunId.
    /// </summary>
    public void GetGunStats(int gunId, out GunCoreStats coreStats, out HeatSinkStats heatSinkStats)
    {
        GunStatsSO targetGun = guns[gunId];

        coreStats = targetGun.GetCoreStats();
        heatSinkStats = targetGun.GetHeatSinkStats();
    }
}
