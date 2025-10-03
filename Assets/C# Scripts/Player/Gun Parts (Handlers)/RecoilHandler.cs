using System.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public class RecoilHandler
{
    private CameraHandler camHandler;
    private PlayerController playerController;
    private float2 toAddRecoil;
    private float2 recoil;


    public void Init(CameraHandler camHandler, PlayerController playerController)
    {
        this.camHandler = camHandler;
        this.playerController = playerController;
    }

    /// <summary>
    /// Ignore recoil recovery for mouse input that moved the camera down (countering the recoil)
    /// </summary>
    public void OnMouseMovement(Vector2 mouseMovement)
    {
        // Only counter if the movement is opposing the recoil
        if (mouseMovement.y != 0f)
        {
            recoil.y = Mathf.MoveTowards(recoil.y, 0f, math.abs(mouseMovement.y));
        }

        if (mouseMovement.x != 0f)
        {
            float recoilX = recoil.x;
            recoil.x = Mathf.MoveTowards(recoil.x, 0f, math.abs(mouseMovement.x));

            float recoilRecovery = recoilX - recoil.x;

            playerController.transform.Rotate(Vector3.up * recoilRecovery);

            Vector3 toAddEuler = new Vector3(0, -recoilRecovery, 0f);
            camHandler.AddRotationToMain(toAddEuler);
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

        float2 maxAddedRecoil = toAddRecoil - new float2(
            Mathf.MoveTowards(toAddRecoil.x, 0f, recoilForceThisFrame.x),
            Mathf.MoveTowards(toAddRecoil.y, 0f, recoilForceThisFrame.y)
        );

        toAddRecoil -= maxAddedRecoil;
        recoil += maxAddedRecoil;

        Vector3 toAddEuler = new Vector3(-maxAddedRecoil.y, maxAddedRecoil.x, 0f);
        Vector3 addedRecoil = camHandler.AddRotationToMain(toAddEuler);




        // Fix Recoil to stop recovering recoil that is not added do to looking up

        //recoil += new float2(-addedRecoil.x, addedRecoil.y);
    }



    /// <summary>
    /// Called every frame when gun has not shot long enough, so recoil start recovering.
    /// </summary>
    public void StabilizeRecoil(float maxRecoilRecovery)
    {
        if (recoil.IsZero()) return;

        float2 recoilRecovery = GetRecoilRecovery(recoil, float2.zero, maxRecoilRecovery);

        recoil -= recoilRecovery;

        Vector3 toAddEuler = new Vector3(recoilRecovery.y, -recoilRecovery.x, 0f);
        camHandler.AddRotationToMain(toAddEuler);
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
