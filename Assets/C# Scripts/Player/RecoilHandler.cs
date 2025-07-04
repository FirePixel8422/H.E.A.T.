using UnityEngine;
using UnityEngine.InputSystem;



public class RecoilHandler : MonoBehaviour
{
    [Header("Transform used for camera movement and now also for recoil")]
    [SerializeField] private Transform cameraTransform;

    private float toAddRecoil;
    private float recoil;


    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        // Ignore recoil recovery for mouse input that moved the camera down (countering the recoil)
        recoil = Mathf.MoveTowards(recoil, 0, -Mathf.Min(ctx.ReadValue<Vector2>().y, 0));
    }


    public void AddRecoil(float recoil)
    {
        toAddRecoil += recoil;
    }


    // Called after the gun executes its OnUpdate
    public void OnUpdate(float recoilForce)
    {
        if (toAddRecoil == 0) return;

        float addedRecoil = toAddRecoil - Mathf.MoveTowards(toAddRecoil, 0, recoilForce * Time.deltaTime);

        toAddRecoil -= addedRecoil;

        //apply the added recoil (up == transform.left)
        cameraTransform.Rotate(Vector3.left, addedRecoil);

        //update how much recoil has been added, for the recovery process
        recoil += addedRecoil;
    }


    /// <summary>
    /// Called every frame when gun has not shot long enough, so recoil start recovering.
    /// </summary>
    public void StabilizeRecoil(float maxRecoilRecovery)
    {
        if (recoil == 0) return;

        //recover up to maxRecoilRecovery
        float recoilRecovery = recoil - Mathf.MoveTowards(recoil, 0, maxRecoilRecovery * Time.deltaTime);
        recoil -= recoilRecovery;

        //apply the recoil recovery (down == transform.right)
        cameraTransform.Rotate(Vector3.right, recoilRecovery);
    }
}
