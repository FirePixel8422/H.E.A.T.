using System.Globalization;
using UnityEngine;


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
    /// Swap gun and get gunstats by gunId.
    /// </summary>
    public void SwapGun(Transform gunParentTransform, int gunId, bool isOwner, ref GunRefHolder gunRefHolder, out GunCoreStats coreStats, out HeatSinkStats heatSinkStats, out GunShakeStats shakeStats, out GunSwayStats swayStats)
    {
        currentGunId = gunId;

        if (gunRefHolder != null)
        {
            gunRefHolder.DestroyGun();
        }

        GunStatsSO targetGun = guns[gunId];

        gunRefHolder = Instantiate(targetGun.GunPrefab, gunParentTransform);
        gunRefHolder.Init(isOwner);

        targetGun.GetGunStats(out coreStats, out heatSinkStats, out shakeStats, out swayStats);
    }

    public void SwapToNextGun(Transform gunParentTransform, bool isOwner, ref GunRefHolder gunRefHolder, out GunCoreStats coreStats, out HeatSinkStats heatSinkStats, out GunShakeStats shakeStats, out GunSwayStats swayStats, out int gunId)
    {
        currentGunId = (currentGunId + 1) % guns.Length;
        gunId = currentGunId;

        SwapGun(gunParentTransform, gunId, isOwner, ref gunRefHolder, out coreStats, out heatSinkStats, out shakeStats, out swayStats);
    }
}
