using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public class GunSwayHandler
{
    [SerializeField] private Transform gunTransform;
    private ADSHandler adsHandler;

    public GunSwayStats stats;

    [Header("Settings")]
    public float2 movementAmplitude = 0.05f;   // height of the bob
    public float movementFrequency = 6f;      // speed of the bob

    [Header("Simulates Breathing")]
    public float2 idleAmplitude = 0.025f;
    public float idleFrequency = 0.5f;

    [Header("Sway")]
    public float movementSway = 2;

    public float offsetSmooth = 12f;
    public float swayRecoverSmooth = 12f;

    private Vector3 startPos;
    private Vector3 swayOffset;
    private float bobTimer;


    public void SwapGun(Transform gunTransform, ADSHandler adsHandler)
    {
        stats.BakeAllCurves();

        this.gunTransform = gunTransform;
        this.adsHandler = adsHandler;

        startPos = gunTransform.localPosition;
    }

    /// <summary>
    /// MovementState affects sway, speed percent01 of maxSpeed and IsAirborne are used for sway percentage
    /// </summary>
    public float GetSpreadMultiplierNerf(float percentage)
    {
        return stats.spreadMultplierCurve.Evaluate(percentage);
    }

    public void OnUpdate(Vector2 mouseInput, Vector2 moveDir, float moveSpeed, bool isGrounded, float deltaTime)
    {
        float zoomPercent = adsHandler.ZoomedInPercent;

        Vector3 bobbingOffset;
        Vector3 targetPos = startPos;

        float amplitude =  moveSpeed > 0 ?
            math.lerp(movementAmplitude.y, movementAmplitude.x, zoomPercent) :
            math.lerp(idleAmplitude.y, idleAmplitude.x, zoomPercent);

        float frequency = moveSpeed > 0 ?
            movementFrequency * moveSpeed * deltaTime :
            idleFrequency * deltaTime;

        bobTimer += frequency;

        float bobOffset = math.sin(bobTimer) * amplitude;

        bobbingOffset = new Vector3(0f, bobOffset, 0f);


        swayOffset = VectorLogic.InstantMoveTowards(swayOffset, Vector3.zero, swayRecoverSmooth * deltaTime);

        swayOffset += new Vector3(-mouseInput.x, -mouseInput.y, 0f) * movementSway;

        targetPos = VectorLogic.InstantMoveTowards(targetPos, targetPos + bobbingOffset + swayOffset, offsetSmooth * deltaTime);

        gunTransform.localPosition = targetPos;
    }

    public void Dispose()
    {
        stats.Dispose();
    }
}
