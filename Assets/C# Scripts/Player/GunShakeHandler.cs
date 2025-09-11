using UnityEngine;

[System.Serializable]
public class GunShakeHandler
{
    [SerializeField] private Transform gunParentTransform;
    [SerializeField] private float shakePitch = 5f;        // random pitch kick
    [SerializeField] private float shakeYaw = 2f;          // random yaw sway
    [SerializeField] private float shakeBuildUp = 20f;     // snap into recoil
    [SerializeField] private float shakeDecay = 10f;       // smooth reset

    [Header("Pullback")]
    [SerializeField] private float pullBackDistance = 0.1f; // local Z offset
    [SerializeField] private float pullBackBuildUp = 25f;
    [SerializeField] private float pullBackDecay = 12f;
    [SerializeField] private float pullBackPitchKick = 3f;  // extra upward rotation from pullback

    private Vector3 currentShakeRot;
    private Vector3 targetShakeRot;

    private float currentPullback;
    private float targetPullback;

    private Quaternion startRot;
    private Vector3 startPos;

    public void Init()
    {
        if (gunParentTransform == null)
        {
            DebugLogger.LogError("GunShakeHandler: gunParentTransform is not set!");
            return;
        }

        startRot = gunParentTransform.localRotation;
        startPos = gunParentTransform.localPosition;
    }

    /// <summary>
    /// Call when firing to kick the weapon in rotation + position
    /// </summary>
    public void AddShake(float shakeMultiplier)
    {
        // rotational shake
        targetShakeRot = new Vector3(
            -Random.Range(0f, shakePitch * shakeMultiplier),  // pitch up
            Random.Range(-shakeYaw, shakeYaw * shakeMultiplier),
            0f
        );

        // positional pullback
        targetPullback = pullBackDistance * shakeMultiplier;
    }

    public void OnUpdate(float deltaTime)
    {
        // --- Rotation ---
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

        // --- Position ---
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

        // --- Coupling: pullback also pitches the gun upward ---
        Vector3 finalRot = currentShakeRot;
        finalRot.x -= currentPullback * pullBackPitchKick * 100f;
        // (multiply by 100 to turn small z-movement into a nice angle, tweak as needed)

        // Apply final transform
        gunParentTransform.localRotation = startRot * Quaternion.Euler(finalRot);
        gunParentTransform.localPosition = startPos + Vector3.back * currentPullback;
    }
}
