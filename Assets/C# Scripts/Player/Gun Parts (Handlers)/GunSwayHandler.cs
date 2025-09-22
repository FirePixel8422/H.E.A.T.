using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;



[System.Serializable]
public class GunSwayHandler
{
    [SerializeField] private Transform gunTransform;

    [Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    private Vector3 swayPos;

    [Header("Sway Rotation")]
    [SerializeField] private float rotationStep = 4f;
    [SerializeField] private float maxRotationStep = 5f;
    [SerializeField] private Vector3 swayEulerRot;

    [SerializeField] private float smooth = 10f;
    [SerializeField] private float smoothRot = 12f;

    [Header("Bobbing")]
    [SerializeField] private float speedCurve;
    private float CurveSin { get => Mathf.Sin(speedCurve); }
    private float CurveCos { get => Mathf.Cos(speedCurve); }

    [SerializeField] private Vector3 travelLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 bobLimit = Vector3.one * 0.01f;
    private Vector3 bobPosition;

    [SerializeField] private float bobExaggeration;

    [Header("Bob Rotation")]
    [SerializeField] private Vector3 multiplier;
    private Vector3 bobEulerRotation;

    public GunSwayStats stats;


    /// <summary>
    /// MovementState affects sway, speed percent01 of maxSpeed and IsAirborne are used for sway percentage
    /// </summary>
    public float GetSpreadMultiplierNerf(float percentage)
    {
        return stats.spreadMultplierCurve.Evaluate(percentage);
    }

    public void SwapGun(Transform gunTransform)
    {
        stats.BakeAllCurves();

        this.gunTransform = gunTransform;
    }

    public void OnUpdate(Vector2 mouseInput, Vector2 moveDir, bool isGrounded, float deltaTime)
    {
        Sway(mouseInput);
        SwayRotation(mouseInput);
        BobOffset(moveDir, isGrounded, deltaTime);
        BobRotation(moveDir);

        CompositePositionRotation(deltaTime);
    }

    private void Sway(Vector2 mouseInput)
    {
        Vector3 invertLook = mouseInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertLook;
    }

    private void SwayRotation(Vector2 mouseInput)
    {
        Vector2 invertLook = mouseInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
        swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    private void CompositePositionRotation(float deltaTime)
    {
        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, swayPos + bobPosition, deltaTime * smooth);
        gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation), deltaTime * smoothRot);
    }

    private void BobOffset(Vector2 moveDir, bool isGrounded, float deltaTime)
    {
        speedCurve += deltaTime * (isGrounded ? (moveDir.x + moveDir.y) * bobExaggeration : 1f) + 0.01f;

        bobPosition.x = (CurveCos * bobLimit.x * (isGrounded ? 1 : 0)) - (moveDir.x * travelLimit.x);
        bobPosition.y = (CurveSin * bobLimit.y) - (moveDir.y * travelLimit.y);
        bobPosition.z = -(moveDir.y * travelLimit.z);
    }

    private void BobRotation(Vector2 moveDir)
    {
        if (moveDir == Vector2.zero)
        {
            bobEulerRotation = Vector3.zero;
            return;
        }

        bobEulerRotation = new Vector3(
            multiplier.x * math.sin(2 * speedCurve), 
            multiplier.y * CurveCos, 
            multiplier.z * CurveCos * moveDir.x);
    }


    public void Dispose()
    {
        stats.Dispose();
    }
}