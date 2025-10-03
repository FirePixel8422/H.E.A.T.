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
    public float headShotMultiplier;


    [Header("The maximum distance this weapon can shoot from until damage fully falls off")]
    public float maxEffectiveRange;

    [Header("How many sample to give the damage falloff curve")]
    public NativeSampledAnimationCurve damageFallOffCurve;

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


    #region Recoil (ADS Pattern) and Spread

    [Header("Scoped in recoil pattern and whether to smooth between points")]
    public float2[] adsRecoilPattern;

    public bool invertX;

    [Header("Multipliers on top off the recoil pattern based on ADS state (blended)")]
    public float2 hipRecoilMultiplier;
    public float2 adsRecoilMultiplier;

    [Header("How Aggressive recoil is added, higher is more 'clicky' recoil")]
    public float2 hipRecoilForce;
    public float2 adsRecoilForce;

    [Header("How fast to reset added recoil")]
    public float hipRecoilRecovery;
    public float adsRecoilRecovery;

    [Header("The time that needs to pass while not shooting for the recoil to stabilize")]
    public float recoilRecoveryDelay;

    [Header("Speed at which recoilPattern resets")]
    public float recoilPatternDecayMultiplier;

    /// <summary>
    /// Increases by 1 for every shot fired and decreases by <see cref="adsRecoilRecovery"/> after not shooting for <see cref="recoilRecoveryDelay"/>.
    /// </summary>
    private float recoilModifier;

    public float2 GetRecoil(float adsPercentage)
    {
        int patternPointId = math.clamp((int)math.floor(recoilModifier), 0, adsRecoilPattern.Length - 1);

        recoilModifier += 1;
        stabilityModifier += stabilityLoss * (1 - adsPercentage);

        if (invertX)
        {
            float2 recoil = adsRecoilPattern[patternPointId];
            recoil.x = -recoil.x;

            return recoil * math.lerp(hipRecoilMultiplier, adsRecoilMultiplier, adsPercentage);
        }
        else
        {
            return adsRecoilPattern[patternPointId] * math.lerp(hipRecoilMultiplier, adsRecoilMultiplier, adsPercentage);
        }
    }
    public float2 GetRecoilForce(float adsPercentage)
    {
        return math.lerp(hipRecoilForce, adsRecoilForce, adsPercentage);
    }
    public float GetRecoilRecovery(float adsPercentage)
    {
        return math.lerp(hipRecoilRecovery, adsRecoilRecovery, adsPercentage);
    }
    public void StabilizeRecoil(float deltaTime)
    {
        recoilModifier = math.clamp(recoilModifier - deltaTime * recoilPatternDecayMultiplier, 0, adsRecoilPattern.Length);
    }
    public void StabilizeHipFire(float multiplier, float deltaTime)
    {
        stabilityModifier = math.clamp(stabilityModifier - multiplier * stabilityRecovery * deltaTime, 0, spreadCurve.Length);
    }

    [Header("Spread curve at each shot and spread multiplier")]
    public NativeSampledAnimationCurve spreadCurve;

    public float stabilityLoss;
    public float stabilityRecovery;

    /// <summary>
    /// Increased by 1 * (1 - adsPercentage01) for every shot fired. Decreased when idle or ADS Shooting by <see cref="stabilityRecovery"/>
    /// </summary>
    private float stabilityModifier;

    /// <summary>
    /// Get weapon spread
    /// </summary>
    public float GetSpread(float adsPercentage)
    {
        return math.lerp(spreadCurve.Evaluate(stabilityModifier), 0, adsPercentage);
    }

    #endregion


    [Header("RPM (How fast can this gun shoot in shots per minute)")]
    public float roundsPerMinute;

    [Tooltip("Time between every shot")]
    public float ShootInterval => 60 / roundsPerMinute;


    [Header("Can trigger be held down for continuous fire? (disables inputBufferTime)")]
    public bool autoFire;

    [Header("How much time does the input get buffered, when its pressed too early")]
    public float inputBufferTime;
    public float InputBufferTime => autoFire ? 0 : inputBufferTime;


    /// <summary>
    /// Bake all curves from the AnimationCurves in this struct into their internal float arrays.
    /// </summary>
    public void BakeAllCurves()
    {
        damageFallOffCurve.Bake();
        spreadCurve.Bake();
    }
    public void Dispose()
    {
        damageFallOffCurve.Dispose();
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
        damageFallOffCurve = NativeSampledAnimationCurve.Default,

        bulletSize = 0.05f,
        bulletHoleFXSize = new MinMaxFloat(0.15f, 0.2f),
        bulletHoleFXLifetime = 10f,

        projectileCount = 1,

        burstShots = 0,
        burstShotInterval = 0.1f,

        heatPerShot = 0.1f,

        adsRecoilPattern = new float2[0],
        recoilModifier = 0,
        invertX = false,

        hipRecoilMultiplier = new float2(1, 1),
        hipRecoilRecovery = 0.5f,
        adsRecoilMultiplier = new float2(1, 1),
        adsRecoilRecovery = 0.5f,
        recoilPatternDecayMultiplier = 50,

        hipRecoilForce = new float2(30, 100),
        adsRecoilForce = new float2(30, 100),

        recoilRecoveryDelay = 0.25f,

        spreadCurve = NativeSampledAnimationCurve.Default,
        stabilityLoss = 0.5f,
        stabilityRecovery = 0.5f,
        stabilityModifier = 0,

        inputBufferTime = 0,

        roundsPerMinute = 600f,
        autoFire = true,
    };
}