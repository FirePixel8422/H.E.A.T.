using UnityEngine;



[System.Serializable]
public class CameraHandler
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera gunCamera;


    public void SetFOV(float fov)
    {
        mainCamera.fieldOfView = fov;
        gunCamera.fieldOfView = fov;
    }


    public void ZoomIn(float zoomMultiplier)
    {

    }
}