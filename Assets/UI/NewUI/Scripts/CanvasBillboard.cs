using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    public Camera mainCamera;

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Make the canvas face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
