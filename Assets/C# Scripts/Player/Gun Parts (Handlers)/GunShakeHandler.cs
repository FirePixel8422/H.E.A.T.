using UnityEngine;


[System.Serializable]
public class GunShakeHandler
{
    [SerializeField] private Transform gunParentTransform;

    public GunShakeStats stats;

    private Vector3 currentShakeRot;
    private Vector3 targetShakeRot;

    private float currentPullback;
    private float targetPullback;

    private Quaternion startRot;
    private Vector3 startPos;


    public void Init()
    {
#if UNITY_EDITOR
        if (gunParentTransform == null)
        {
            DebugLogger.LogError("GunShakeHandler: gunParentTransform is not set!");
            return;
        }
#endif

        startRot = gunParentTransform.localRotation;
        startPos = gunParentTransform.localPosition;
    }

    /// <summary>
    /// Call when firing to kick the weapon in rotation and position
    /// </summary>
    public void AddShake(float shakeMultiplier, float zoomedInPercentage)
    {
        shakeMultiplier *= Mathf.Lerp(1, stats.adsMultplier, zoomedInPercentage);

        // rotational shake
        targetShakeRot = new Vector3(
            -Random.Range(0f, stats.shakePitch * shakeMultiplier),  // pitch up
            Random.Range(-stats.shakeYaw, stats.shakeYaw * shakeMultiplier),
            0f
        );

        // positional pullback
        targetPullback = stats.pullBackDistance * shakeMultiplier;
    }

    public void OnUpdate(float deltaTime, float zoomedInPercentage)
    {
        float multiplier = Mathf.Lerp(1, stats.adsMultplier, zoomedInPercentage);

        currentShakeRot = Vector3.Lerp(
            currentShakeRot,
            targetShakeRot,
            deltaTime * stats.shakeBuildUp * multiplier
        );

        targetShakeRot = Vector3.Lerp(
            targetShakeRot,
            Vector3.zero,
            deltaTime * stats.shakeDecay * multiplier
        );

        currentPullback = Mathf.Lerp(
            currentPullback,
            targetPullback,
            deltaTime * stats.pullBackBuildUp * multiplier
        );

        targetPullback = Mathf.Lerp(
            targetPullback,
            0f,
            deltaTime * stats.pullBackDecay * multiplier
        );

        // Pullback also pitches the gun upward
        Vector3 finalRot = currentShakeRot;
        finalRot.x -= currentPullback * stats.pullBackPitchKick * multiplier;

        // Update Transform
        gunParentTransform.SetLocalPositionAndRotation(startPos + Vector3.back * currentPullback * multiplier, startRot * Quaternion.Euler(finalRot));
    }
}
