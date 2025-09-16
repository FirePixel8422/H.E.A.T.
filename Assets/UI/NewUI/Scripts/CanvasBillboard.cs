using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Get the main camera in the scene
        mainCamera = Camera.main;
    }

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
