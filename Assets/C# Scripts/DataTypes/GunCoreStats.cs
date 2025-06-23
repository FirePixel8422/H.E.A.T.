using UnityEngine;



[System.Serializable]
public struct GunCoreStats
{
    public float damage;

    [Header("How much heat to add to the heatsink per shot")]
    public float heatPerShot;

    [Header("How much recoil to add per shot")]
    public float recoilPerShot;

    [Header("The time that needs to pass while not shooting for the recoil to stabilize")]
    public float recoilRecoveryDelay;
    [Header("How much recoil to recover per second while not shooting")]
    public float recoilRecovery;

    [Header("RPM (How fast can this gun shoot in shots per minute)")]
    [SerializeField] private float roundsPerMinute;
    public float ShootInterval => 60 / roundsPerMinute;

    [Header("Can trigger be held down for continuous fire?")]
    public bool autoFire;

    [Header("List of audio clips to play when shooting")]
    public AudioClip[] shootAudioClips;
    public MinMaxFloat minMaxPitch;


    /// <summary>
    /// Deafault values for the gun.
    /// </summary>
    public static GunCoreStats Default => new GunCoreStats()
    {
        damage = 10f,
        heatPerShot = 0.1f,

        recoilPerShot = 0.1f,
        recoilRecoveryDelay = 0.25f,
        recoilRecovery = 0.5f,

        roundsPerMinute = 600f,
        autoFire = true,
    };
}