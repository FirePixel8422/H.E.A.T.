using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Serializable data container that defines how a gun shakes and pulls back when fired.
/// Used by <see cref="GunShakeHandler"/> to drive visual weapon feedback.
/// </summary>
[System.Serializable]
public struct GunShakeStats
{
    [Header("X coord is for HIP, Y coord is for ADS data")]

    [Header("Maximum pitch kick upwards (degrees).")]
    public float2 shakePitch;
    [Header("Maximum yaw kick left/right (degrees).")]
    public float2 shakeYaw;
    [Header("How quickly the rotation kicks in (higher = snappier).")]
    public float2 shakeBuildUp;
    [Header("How quickly the shake settles back to neutral (higher = faster decay).")]
    public float2 shakeDecay;
    
    [Header("How far the gun moves backwards when fired.")]
    public float2 pullBackDistance;
    [Header("How quickly the pullback is applied (higher = snappier).")]
    public float2 pullBackBuildUp;
    [Header("How quickly the gun returns to its starting position after pullback.")]
    public float2 pullBackDecay;
    [Header("Additional upward pitch applied as the gun pulls back (multiplier).")]
    public float2 pullBackPitchKick;


    /// <summary>
    /// Default values for ShakeSTats
    /// </summary>
    public static GunShakeStats Default => new GunShakeStats()
    {
        shakePitch = 5,
        shakeYaw = 2,
        shakeBuildUp = 20,
        shakeDecay = 10,

        pullBackDistance = 0.1f,
        pullBackBuildUp = 25,
        pullBackDecay = 12,
        pullBackPitchKick = 300,
    };
}
