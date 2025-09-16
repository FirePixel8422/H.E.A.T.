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

    public float2 toAddRecoil;
    public float2 recoil;

    public float2 addedRecoil;
    public float2 removedRecoil;
    public float2 neutralizedRecoil;
    public float2 totalRecoilRecov;


    /// <summary>
    /// Ignore recoil recovery for mouse input that moved the camera down (countering the recoil)
    /// </summary>
    public void OnMouseMovement(Vector2 mouseMovement)
    {
        // Only counter if the movement is opposing the recoil
        if (mouseMovement.y < 0f)
        {
            float recoilY = recoil.y;
            recoil.y = Mathf.MoveTowards(recoil.y, 0f, -mouseMovement.y);

            neutralizedRecoil.y += recoilY - recoil.y;
        }

        if (mouseMovement.x != 0f)
        {
            float recoilX = recoil.x;
            recoil.x = Mathf.MoveTowards(recoil.x, 0f, math.abs(mouseMovement.x));

            float recoilRecovery = recoilX - recoil.x;

            PlayerController.LocalInstance.transform.Rotate(Vector3.up * recoilRecovery);


            Vector3 currentEuler = recoilTransform.localEulerAngles;
            Vector3 newRotation = new Vector3(
                currentEuler.x,
                currentEuler.y - recoilRecovery,
                0f
            );
            recoilTransform.localEulerAngles = newRotation;

            neutralizedRecoil.x += recoilRecovery;
        }

        totalRecoilRecov = removedRecoil + neutralizedRecoil;
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

        this.addedRecoil += addedRecoil;

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
            return;

            // Reset transform
            Quaternion cRot = recoilTransform.localRotation;
            Vector3 cEuler = recoilTransform.localEulerAngles;

            Quaternion fixedRotation = Quaternion.RotateTowards(cRot, Quaternion.Euler(new Vector3(cEuler.x, 0, 0)), maxRecoilRecovery);
            recoilTransform.localRotation = fixedRotation;

            return;
        }

        float2 recoilRecovery = GetRecoilRecovery(recoil, float2.zero, maxRecoilRecovery);

        recoil -= recoilRecovery;
        this.removedRecoil += recoilRecovery;
        totalRecoilRecov = removedRecoil + neutralizedRecoil;

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
