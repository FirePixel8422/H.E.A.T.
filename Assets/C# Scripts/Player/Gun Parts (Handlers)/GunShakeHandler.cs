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
    public void AddShake(float shakeMultiplier, float adsPercentage)
    {
        float shakePitch = Mathf.Lerp(stats.shakePitch.x, stats.shakePitch.y, adsPercentage);
        float shakeYaw = Mathf.Lerp(stats.shakeYaw.x, stats.shakeYaw.y, adsPercentage);
        float pullBackDistance = Mathf.Lerp(stats.pullBackDistance.x, stats.pullBackDistance.y, adsPercentage);

        // rotational shake
        targetShakeRot = new Vector3(
            -Random.Range(0f, shakePitch) * shakeMultiplier,  // pitch up
            Random.Range(-shakeYaw, shakeYaw) * shakeMultiplier,
            0f
        );

        // positional pullback
        targetPullback = pullBackDistance * shakeMultiplier;
    }

    public void OnUpdate(float deltaTime, float adsPercentage)
    {
        float shakeBuildUp = Mathf.Lerp(stats.shakeBuildUp.x, stats.shakeBuildUp.y, adsPercentage);
        float shakeDecay = Mathf.Lerp(stats.shakeDecay.x, stats.shakeDecay.y, adsPercentage);
        float pullBackBuildUp = Mathf.Lerp(stats.pullBackBuildUp.x, stats.pullBackBuildUp.y, adsPercentage);
        float pullBackDecay = Mathf.Lerp(stats.pullBackDecay.x, stats.pullBackDecay.y, adsPercentage);
        float pullBackPitchKick = Mathf.Lerp(stats.pullBackPitchKick.x, stats.pullBackPitchKick.y, adsPercentage);

        currentShakeRot = Vector3.Lerp(
            currentShakeRot,
            targetShakeRot,
            deltaTime * shakeBuildUp
        );

        targetShakeRot = Vector3.Lerp(
            targetShakeRot,
            Vector3.zero,
            deltaTime * shakeDecay
        );

        currentPullback = Mathf.Lerp(
            currentPullback,
            targetPullback,
            deltaTime * pullBackBuildUp
        );

        targetPullback = Mathf.Lerp(
            targetPullback,
            0f,
            deltaTime * pullBackDecay
        );

        // Pullback also pitches the gun upward
        Vector3 finalRot = currentShakeRot;
        finalRot.x -= currentPullback * pullBackPitchKick;

        // Update Transform
        gunParentTransform.SetLocalPositionAndRotation(startPos + Vector3.back * currentPullback, startRot * Quaternion.Euler(finalRot));
    }
}
