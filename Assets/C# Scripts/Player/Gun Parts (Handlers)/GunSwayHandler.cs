using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public class GunSwayHandler
{
    [SerializeField] private Transform gunTransform;
    private ADSHandler adsHandler;

    public GunSwayStats stats = GunSwayStats.Default;

    private Vector3 startPos;
    private Vector3 startEuler;
    private Vector3 swayPosOffset;
    private Vector3 swayEulerOffset;
    private float bobTimer;


    public void OnSwapGun(Transform gunTransform, ADSHandler adsHandler)
    {
        this.gunTransform = gunTransform;
        this.adsHandler = adsHandler;

        startPos = gunTransform.localPosition + Vector3.up * stats.gunYOffset;
        startEuler = gunTransform.localEulerAngles;
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
        //DebugLogger.Log(moveDir);

        float zoomPercent = adsHandler.ZoomedInPercent;
        Vector3 targetPos = startPos;


        #region Bobbing Effect

        Vector3 bobbingOffset;

        float amplitude =  moveSpeed > 0 ?
            math.lerp(stats.movementAmplitude.x, stats.movementAmplitude.y, zoomPercent) :
            math.lerp(stats.idleAmplitude.x, stats.idleAmplitude.y, zoomPercent);

        float frequency = moveSpeed > 0 ?
            stats.movementFrequency * moveSpeed * deltaTime :
            stats.idleFrequency * deltaTime;

        bobTimer += frequency;

        float bobOffset = math.sin(bobTimer) * amplitude;
        bobbingOffset = new Vector3(0f, bobOffset, 0f);

        targetPos = VectorLogic.InstantMoveTowards(targetPos, targetPos + bobbingOffset, stats.offsetSmooth * deltaTime);

        #endregion


        #region Sway Effect

        float posSwayMouse = math.lerp(stats.posSwayMouse.x, stats.posSwayMouse.y, zoomPercent);
        float eulerSwayMouse = math.lerp(stats.eulerSwayMouse.x, stats.eulerSwayMouse.y, zoomPercent);
        float posSwayMove = math.lerp(stats.posSwayMove.x, stats.posSwayMove.y, zoomPercent);
        float eulerSwayMove = math.lerp(stats.eulerSwayMove.x, stats.eulerSwayMove.y, zoomPercent);

        // Lerp previous sway back to 0 and add new mouseInput to sway
        swayPosOffset = Vector3.Lerp(swayPosOffset, Vector3.zero, stats.swayRecoverSmooth * deltaTime);
        swayEulerOffset = Vector3.Lerp(swayEulerOffset, Vector3.zero, stats.swayRecoverSmooth * deltaTime);

        swayPosOffset += new Vector3(-mouseInput.x, -mouseInput.y, 0f) * posSwayMouse + new Vector3(-moveDir.x, stats.ignoreYForSwayMove ? 0 : -moveDir.y, 0) * posSwayMove;
        swayEulerOffset += new Vector3(-mouseInput.y, mouseInput.x, 0f) * eulerSwayMouse + new Vector3(-moveDir.x, stats.ignoreYForSwayMove ? 0 : -moveDir.y, 0) * eulerSwayMove;

        #endregion


        gunTransform.SetLocalPositionAndRotation(targetPos + swayPosOffset, Quaternion.Euler(startEuler + swayEulerOffset));
    }
}
