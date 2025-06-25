using UnityEngine;
using UnityEngine.InputSystem;



public class RecoilHandler : MonoBehaviour
{
    [SerializeField] private Transform recoilTransform;

    private float toAddRecoil;
    private float recoil;

    [SerializeField] private float recoilSmoothSpeed = 15f;

    private const float epsilon = 0.001f;



    public void OnLook(InputAction.CallbackContext ctx)
    {
        //if any mouseMovement is present
        if (ctx.performed && ctx.ReadValue<Vector2>() != Vector2.zero)
        {
            recoil = 0;
        }
    }


    public void AddRecoil(float recoil)
    {
        toAddRecoil += recoil;
    }


    public void OnUpdate()
    {
        if (toAddRecoil == 0) return;

        float addedRecoil = toAddRecoil - Mathf.MoveTowards(toAddRecoil, 0, recoilSmoothSpeed * Time.deltaTime);

        toAddRecoil -= addedRecoil;

        //check if remaining toAddRecoil is smaller than epsilon, if so, set it to 0 and add the difference to addedRecoil
        if (toAddRecoil < epsilon)
        {
            addedRecoil += epsilon - toAddRecoil;

            toAddRecoil = 0;
        }

        //apply the added recoil (up == transform.left)
        recoilTransform.Rotate(Vector3.left, addedRecoil);

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

        //check if remaining recoil is smaller than epsilon, if so, set it to 0 and add the difference to recoilRecovery
        if (recoil < epsilon)
        {
            recoilRecovery += epsilon - recoil;

            recoil = 0;
        }

        //apply the recoil recovery (down == transform.right)
        recoilTransform.Rotate(Vector3.right, recoilRecovery);
    }
}
