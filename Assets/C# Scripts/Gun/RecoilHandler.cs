using UnityEngine;
using UnityEngine.InputSystem;



public class RecoilHandler : MonoBehaviour
{
    [SerializeField] private Transform recoilTransform;

    private float cRecoil;
    private float previousVisualRecoil;
    private float currentVisualRecoil;

    [SerializeField] private float recoilSmoothSpeed = 15f;

    private const float epsilon = 0.001f;



    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 mouseDelta = ctx.ReadValue<Vector2>();

            if (mouseDelta.y < 0)
            {
                float diff = currentVisualRecoil - Mathf.MoveTowards(currentVisualRecoil, 0, -mouseDelta.y);

                cRecoil = Mathf.MoveTowards(cRecoil, 0, diff);
                previousVisualRecoil = Mathf.MoveTowards(previousVisualRecoil, 0, diff);
                currentVisualRecoil = Mathf.MoveTowards(currentVisualRecoil, 0, diff);
            }
        }
    }


    public void AddRecoil(float recoil)
    {
        cRecoil += recoil;
    }


    public void OnUpdate()
    {
        currentVisualRecoil = Mathf.Lerp(currentVisualRecoil, cRecoil, recoilSmoothSpeed * Time.deltaTime);

        float deltaRecoil = currentVisualRecoil - previousVisualRecoil;

        // Rotate the recoil transform around local X axis by the delta recoil
        recoilTransform.Rotate(Vector3.left, deltaRecoil);

        previousVisualRecoil = currentVisualRecoil;
    }

    public void StabilizeRecoil(float maxRecoilRecovery)
    {
        if (cRecoil == 0) return;

        //recover up to maxRecoilRecovery
        float recoilRecovery = cRecoil - Mathf.MoveTowards(cRecoil, 0, maxRecoilRecovery * Time.deltaTime);
        cRecoil -= recoilRecovery;

        //check if remaining recoil is smaller than epsilon, if so, set it to 0 and add the difference to recoilRecovery
        if (cRecoil < epsilon)
        {
            recoilRecovery += epsilon - cRecoil;

            cRecoil = 0;
        }
    }
}
