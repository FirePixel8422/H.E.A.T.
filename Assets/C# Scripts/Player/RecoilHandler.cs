using System.Collections;
using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public class RecoilHandler
{
    [Header("Transform used for camera movement and now also for recoil")]
    [SerializeField] private Transform cameraTransform;

    [Space(10)]

    private float2 toAddRecoil;
    private float2 recoil;


    /// <summary>
    /// Ignore recoil recovery for mouse input that moved the camera down (countering the recoil)
    /// </summary>
    public void OnMouseMovement(Vector2 mouseMovement)
    {
        // Only counter if the movement is opposing the recoil
        if (mouseMovement.y < 0f) recoil.y = Mathf.MoveTowards(recoil.y, 0f, -mouseMovement.y);
        if (mouseMovement.x != 0f) recoil.x = Mathf.MoveTowards(recoil.x, 0f, Mathf.Abs(mouseMovement.x));
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

        // Apply recoil (pitch = vertical, yaw = horizontal)
        cameraTransform.Rotate(Vector3.left, addedRecoil.y);    // vertical
        cameraTransform.Rotate(Vector3.up, addedRecoil.x);      // horizontal

        recoil += addedRecoil;
    }



    /// <summary>
    /// Called every frame when gun has not shot long enough, so recoil start recovering.
    /// </summary>
    public void StabilizeRecoil(float2 maxRecoilRecovery)
    {
        if (recoil.IsZero()) return;

        // Recover up to maxRecoilRecovery
        float2 recoilRecovery = recoil - new float2(
            Mathf.MoveTowards(recoil.x, 0, maxRecoilRecovery.x),
            Mathf.MoveTowards(recoil.y, 0, maxRecoilRecovery.y)
        );

        recoil -= recoilRecovery;

        // Apply the recoil recovery (down == transform.right)
        cameraTransform.Rotate(Vector3.right, recoilRecovery.y); // vertical down
        cameraTransform.Rotate(Vector3.down, recoilRecovery.x);  // horizontal back
    }
}
