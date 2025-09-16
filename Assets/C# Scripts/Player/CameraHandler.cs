using UnityEngine;



[System.Serializable]
public class CameraHandler
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera gunCamera;

    [SerializeField] private mainCamMaxTiltAngle


    public void SetFOV(float fov)
    {
        mainCamera.fieldOfView = fov;
        gunCamera.fieldOfView = fov;
    }


    public void ZoomIn(float zoomMultiplier)
    {

    }

    public void SendRotationToMain(Quaternion rot)
    {
        Vector3 euler = rot.eulerAngles;
        euler.x = 

        camEuler.x = math.clamp(camEuler.x, -maxTiltAngle, maxTiltAngle);

        mainCamera.transform.rotation = 
    }
}