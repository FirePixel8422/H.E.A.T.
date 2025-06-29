using UnityEngine;


[System.Serializable]
public struct GunCoreStats
{
    [Header("How big is the bullet (as diameter)")]
    public float bulletSize;
    [Header("How big is the bulletHole that is left from this bullet (scale)")]
    public float bulletHoleFXSize;


    [Header("Damage per bullet")]
    public float damage;
    [Header("When headshot damage is done, multiply by this value")]
    public float headShotMultiplier;


    [Header("How much heat to add to the heatsink per shot")]
    public float heatPerShot;


    [Header("How much recoil to add per shot")]
    public float recoilPerShot;
    [Header("How Aggressive recoil is added, higher is more 'clicky' recoil")]
    public float recoilForce;

    [Header("The time that needs to pass while not shooting for the recoil to stabilize")]
    public float recoilRecoveryDelay;
    [Header("How much recoil to recover per second while not shooting")]
    public float recoilRecovery;


    [Header("How much the bullet can maximally offset from actual shot point")]
    public float maxSpread;

    [Header("How many sample to give the spread curve, more samples, higher accuracy spread curve")]
    [Range(2, 50)]
    public int spreadCurveSampleCount;
    [Header(">>DEBUG<<, the baked spread curve, used for the actual spread calculation instead of the animation curve")]
    public float[] bakedSpreadCurve;

    public float GetSpread(float heatPercentage)
    {
        int index = (int)(heatPercentage * (spreadCurveSampleCount - 1));
        index = Mathf.Clamp(index, 0, spreadCurveSampleCount - 1);
        return bakedSpreadCurve[index];
    }


    [Header("RPM (How fast can this gun shoot in shots per minute)")]
    [SerializeField] private float roundsPerMinute;

    [Tooltip("Time between every shot")]
    public float ShootInterval => 60 / roundsPerMinute;


    [Header("Can trigger be held down for continuous fire? (disables inputBufferTime)")]
    public bool autoFire;

    [Header("How much time does the input get buffered, when its pressed too early")]
    [SerializeField] private float inputBufferTime;
    public float InputBufferTime => autoFire ? 0 : inputBufferTime;


    [Header("List of audio clips to randomly pick and play when shooting")]
    public AudioClip[] shootAudioClips;
    public MinMaxFloat minMaxPitch;


    /// <summary>
    /// Default values for the gun.
    /// </summary>
    public static GunCoreStats Default => new GunCoreStats()
    {
        bulletSize = 0.05f,
        bulletHoleFXSize = 0.2f,

        damage = 10f,
        headShotMultiplier = 1.5f,

        heatPerShot = 0.1f,

        recoilPerShot = 0.1f,
        recoilForce = 15f,

        recoilRecoveryDelay = 0.25f,
        recoilRecovery = 0.5f,

        maxSpread = 0.25f,
        spreadCurveSampleCount = 12,
        bakedSpreadCurve = new float[0],

        inputBufferTime = 0,

        roundsPerMinute = 600f,
        autoFire = true,

        shootAudioClips = new AudioClip[1],
        minMaxPitch = new MinMaxFloat(0.95f, 1.05f),
    };
}