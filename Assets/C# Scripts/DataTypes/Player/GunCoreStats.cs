using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Core stats of any Gun (Recoil, spread, damage, RPM, reloadTime, falloff, etc)
/// </summary>
[System.Serializable]
public struct GunCoreStats
{
    [Header("Damage per bullet")]
    [Tooltip("Use GetDamageOutput() for true damage. Raw damage dealt per bullet, before falloff and headshot multiplier")]
    public float damage;
    [Header("When headshot damage is done, multiply by this value")]
    [SerializeField] private float headShotMultiplier;


    [Header("The maximum distance this weapon can shoot from until damage fully falls off")]
    [SerializeField] private float maxEffectiveRange;

    [Header("How many sample to give the damage falloff curve")]
    [SerializeField] private SampledAnimationCurve damageFallOffCurve;

    /// <summary>
    /// Get damage output and account for headShot and damage falloff
    /// </summary>
    public float GetDamageOutput(float distance, bool isHeadshot)
    {
        float fallOffMultiplier = damageFallOffCurve.Evaluate(distance / maxEffectiveRange);
        float critMultiplier = isHeadshot ? headShotMultiplier : 1f;

        return damage * fallOffMultiplier * critMultiplier;
    }


    [Header("How big is the bullet (as diameter)")]
    public float bulletSize;
    [Header("How big is the bulletHole that is left from this bullet (scale)")]
    public MinMaxFloat bulletHoleFXSize;
    [Header("How long the bullet hole FX stays before despawning")]
    public float bulletHoleFXLifetime;


    [Header("How much projectiles to fire per shot (e.g. shotgun pellets)")]
    public int projectileCount;

    [Header("How many additonal shots to fire in sequence after shooting (0 == no burst)")]
    [Range(0, 9)]
    public int burstShots;
    [Header("The time between each burst projectile")]
    public float burstShotInterval;


    [Header("How much heat to add to the heatsink per shot")]
    public float heatPerShot;

    [Header("How long the recoilPattern is")]
    public float shootIntensityGainMultplier;
    public float shootIntensityDescreaseMultplier;

    [Header("How much recoil to add per shot")]
    [SerializeField] private float hipFireRecoilPerShot;
    [Header("Time shooting to reach the last value of the recoilMultiplierCurve")]
    [SerializeField] private float timeToMinRecoil;

    [Header("How Aggressive recoil is added, higher is more 'clicky' recoil")]
    public float2 recoilForce;

    [Header("How many sample to give the recoil multiplier curve")]
    [SerializeField] private SampledAnimationCurve hipFireRecoilMultiplierCurve;

    public float GetHipFireRecoil(float recoilPatternPercent)
    {
        recoilPatternPercent.Saturated();

        return hipFireRecoilMultiplierCurve.Evaluate(Mathf.Clamp(recoilPatternPercent, 0, timeToMinRecoil) / timeToMinRecoil) * hipFireRecoilPerShot;
    }

    [Header("Scoped in recoil pattern and whether to smooth between points")]
    public float2[] adsRecoilPattern;

    [SerializeField] private bool invertX;
    [SerializeField] private float2 adsRecoilMultiplier;
    [SerializeField] private float adsRecoilDecayMultiplier;

    private float cADSRecoilIdFloat;

    public float2 GetADSRecoil()
    {
        int patternPointId = math.clamp((int)math.floor(cADSRecoilIdFloat), 0, adsRecoilPattern.Length - 1);

        cADSRecoilIdFloat += 1;

        if (invertX)
        {
            float2 recoil = adsRecoilPattern[patternPointId];
            recoil.x = -recoil.x;

            return recoil * adsRecoilMultiplier;
        }
        else
        {
            return adsRecoilPattern[patternPointId] * adsRecoilMultiplier;
        }
    }
    public void DecreaseADSRecoil(float deltaTime)
    {
        cADSRecoilIdFloat = math.clamp(cADSRecoilIdFloat - deltaTime * adsRecoilDecayMultiplier, 0, adsRecoilPattern.Length);
    }

    [Header("The time that needs to pass while not shooting for the recoil to stabilize")]
    public float recoilRecoveryDelay;
    [Header("How much recoil to recover per second while not shooting")]
    public float2 recoilRecovery;


    [Header("How much the bullet can maximally offset from actual shot point")]
    public float maxSpread;

    [Header(">>DEBUG<<, the baked spread curve, used for the actual spread calculation instead of the animation curve")]
    [SerializeField] private SampledAnimationCurve spreadCurve;

    /// <summary>
    /// Get weapon spread
    /// </summary>
    public float GetHipFireSpread(float shootingIntensity)
    {
        return spreadCurve.Evaluate(shootingIntensity) * maxSpread;
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
    public MinMaxFloat minMaxPitchAtMaxHeat;

    [Header("Audio clip to play when gun overheats")]
    public AudioClip overHeatAudioClip;
    public MinMaxFloat overHeatMinMaxPitch;


    /// <summary>
    /// Bake all curves from the editor AnimationCurve to the internal float array.
    /// </summary>
    public void BakeAllCurves()
    {
        damageFallOffCurve.Bake();
        hipFireRecoilMultiplierCurve.Bake();
        spreadCurve.Bake();
    }

    /// <summary>
    /// Free all allocated native mmemory underlying in this struct
    /// </summary>
    public void Dispose()
    {
        damageFallOffCurve.Dispose();
        hipFireRecoilMultiplierCurve.Dispose();
        spreadCurve.Dispose();
    }

    /// <summary>
    /// Default values for the gun.
    /// </summary>
    public static GunCoreStats Default => new GunCoreStats()
    {
        damage = 10f,
        headShotMultiplier = 1.5f,

        maxEffectiveRange = 25,
        damageFallOffCurve = SampledAnimationCurve.Default(),

        bulletSize = 0.05f,
        bulletHoleFXSize = new MinMaxFloat(0.15f, 0.2f),
        bulletHoleFXLifetime = 10f,

        projectileCount = 1,

        burstShots = 0,
        burstShotInterval = 0.1f,

        heatPerShot = 0.1f,

        shootIntensityGainMultplier = 1,
        shootIntensityDescreaseMultplier = 1,

        hipFireRecoilPerShot = 0.1f,
        hipFireRecoilMultiplierCurve = SampledAnimationCurve.Default(),

        adsRecoilPattern = new float2[0],
        cADSRecoilIdFloat = 0,
        invertX = false,
        adsRecoilMultiplier = new float2(1, 1),
        adsRecoilDecayMultiplier = 10,

        recoilForce = new float2(25, 50),
        timeToMinRecoil = 7.5f,

        recoilRecoveryDelay = 0.25f,
        recoilRecovery = 0.5f,

        maxSpread = 0.25f,
        spreadCurve = SampledAnimationCurve.Default(),

        inputBufferTime = 0,

        roundsPerMinute = 600f,
        autoFire = true,

        shootAudioClips = new AudioClip[1],
        minMaxPitch = new MinMaxFloat(0.95f, 1.05f),
        minMaxPitchAtMaxHeat = new MinMaxFloat(0.95f, 1.05f),

        overHeatAudioClip = null,
        overHeatMinMaxPitch = new MinMaxFloat(0.95f, 1.05f),
    };
}