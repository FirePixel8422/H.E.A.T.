using System.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public class RecoilHandler
{
    [Header("Transform used for camera recoil")]
    [SerializeField] private Transform recoilTransform;

    [Space(10)]

    private float2 toAddRecoil;
    private float2 recoil;


    /// <summary>
    /// Ignore recoil recovery for mouse input that moved the camera down (countering the recoil)
    /// </summary>
    public void OnMouseMovement(Vector2 mouseMovement)
    {
        // Only counter if the movement is opposing the recoil
        if (mouseMovement.y < 0f)
        {
            float newRecoilY = Mathf.MoveTowards(recoil.y, 0f, -mouseMovement.y);
            recoil.y = newRecoilY;
        }

        if (mouseMovement.x != 0f)
        {
            float newRecoilX = Mathf.MoveTowards(recoil.x, 0f, math.abs(mouseMovement.x));
            float recoilXDiff = recoil.x - newRecoilX;

            PlayerController.LocalInstance.transform.Rotate(Vector3.up * recoilXDiff);
            recoilTransform.Rotate(-Vector3.up * recoilXDiff);

            recoil.x = newRecoilX;
        }
    }


    /// <summary>
    /// Ready recoil to be added to the camera transform.
    /// </summary>
    public IEnumerator AddRecoil(float2 recoil, float shootInterval)
    {
        toAddRecoil += recoil * 1.5f;

        yield return new WaitForSeconds(shootInterval * 0.5f);

        toAddRecoil -= recoil * 0.5f;
    }

    // Called after the gun executes its OnUpdate
    public void OnUpdate(float2 recoilForceThisFrame)
    {
        if (toAddRecoil.IsZero()) return;

        float2 addedRecoil = toAddRecoil - new float2(
            Mathf.MoveTowards(toAddRecoil.x, 0f, recoilForceThisFrame.x),
            Mathf.MoveTowards(toAddRecoil.y, 0f, recoilForceThisFrame.y)
        );

        toAddRecoil -= addedRecoil;
        recoil += addedRecoil;

        Vector3 currentEuler = recoilTransform.localEulerAngles;
        Vector3 newRotation = new Vector3(
            currentEuler.x - addedRecoil.y,
            currentEuler.y + addedRecoil.x,
            0f
        );
        recoilTransform.localEulerAngles = newRotation;
    }



    /// <summary>
    /// Called every frame when gun has not shot long enough, so recoil start recovering.
    /// </summary>
    public void StabilizeRecoil(float maxRecoilRecovery)
    {
        if (recoil.IsZero())
        {
            // Reset transform
            Vector3 cEuler = recoilTransform.localEulerAngles;
            cEuler.NormalizeAsEuler();

            Vector3 fixedRotation = VectorLogic.InstantMoveTowards(cEuler, new Vector3(cEuler.x, 0, 0), maxRecoilRecovery);
            recoilTransform.localEulerAngles = fixedRotation;

            return;
        }

        float2 recoilRecovery = GetRecoilRecovery(recoil, float2.zero, maxRecoilRecovery);

        recoil -= recoilRecovery;

        // Apply the recoil recovery
        Vector3 currentEuler = recoilTransform.localEulerAngles;
        Vector3 newRotation = new Vector3(
            currentEuler.x + recoilRecovery.y,  // positive because we're recovering
            currentEuler.y - recoilRecovery.x,  // negative because we're recovering
            0f
        );
        recoilTransform.localEulerAngles = newRotation;
    }


    [BurstCompile(DisableSafetyChecks = true)]
    public static float2 GetRecoilRecovery(float2 from, float2 to, float maxStep)
    {
        float2 direction = from - to;
        float distance = math.length(direction);

        if (distance <= maxStep)
        {
            return math.normalize(direction) * distance;
        }

        return math.normalize(direction) * maxStep;
    }
}
