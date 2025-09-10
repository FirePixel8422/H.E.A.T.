using UnityEngine;

[System.Serializable]
public class GunShakeHandler
{
    [SerializeField] private Transform gunParentTransform;
    [SerializeField] private float shakePitch = 5f;     // up/down tilt
    [SerializeField] private float shakeYaw = 2f;       // left/right sway
    [SerializeField] private float shakeBuildUp = 20f;  // snap into recoil
    [SerializeField] private float shakeDecay = 10f;    // smooth reset

    private Vector3 currentShakeRot;
    private Vector3 targetShakeRot;
    private Quaternion startRot;

    public void Init()
    {
        if (gunParentTransform == null)
        {
            DebugLogger.LogError("GunShakeHandler: gunParentTransform is not set!");
            return;
        }

        startRot = gunParentTransform.localRotation;
    }

    /// <summary>
    /// Call when firing to kick the weapon in rotation
    /// </summary>
    public void AddShake(float shakeMultiplier)
    {
        targetShakeRot = new Vector3(
            -Random.Range(0f, shakePitch * shakeMultiplier),  // pitch up (negative X in Unity)
            Random.Range(-shakeYaw, shakeYaw * shakeMultiplier),
            0f
        );
    }

    public void OnUpdate(float deltaTime)
    {
        // Lerp current rotation toward target
        currentShakeRot = Vector3.Lerp(
            currentShakeRot,
            targetShakeRot,
            deltaTime * shakeBuildUp
        );

        // Decay target back to zero
        targetShakeRot = Vector3.Lerp(
            targetShakeRot,
            Vector3.zero,
            deltaTime * shakeDecay
        );

        // Apply final rotation
        gunParentTransform.localRotation = startRot * Quaternion.Euler(currentShakeRot);
    }
}
