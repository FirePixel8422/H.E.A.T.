using UnityEngine;


public class GunManager : MonoBehaviour
{
    public static GunManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }


    [SerializeField] private GunStatsSO[] baseGuns;
    private int currentGunId;

    /// <summary>
    /// Swap gun and get gunstats by gunId.
    /// </summary>
    public void SwapGun(
        Transform gunParentTransform, int gunId, bool isOwner, ref GunRefHolder gunRefHolder,
        out GunCoreStats coreStats,
        out HeatSinkStats heatSinkStats,
        out GunShakeStats shakeStats,
        out GunSwayStats swayStats,
        out GunADSStats gunADSStats)
    {
        currentGunId = gunId;

        if (gunRefHolder != null)
        {
            gunRefHolder.DestroyGun();
        }

        GunStatsSO targetGun = baseGuns[gunId];

        gunRefHolder = Instantiate(targetGun.GunPrefab, gunParentTransform);
        gunRefHolder.Init(isOwner);

        targetGun.GetGunStats(out coreStats, out heatSinkStats, out shakeStats, out swayStats, out gunADSStats);
    }

    public int GetNextGunId() 
    {
        currentGunId = (currentGunId + 1) % baseGuns.Length;
        return currentGunId;
    }


    public string GetCurrentGunName() => baseGuns[currentGunId].name;
}
