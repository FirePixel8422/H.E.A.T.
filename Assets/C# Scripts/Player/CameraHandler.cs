using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public class CameraHandler
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera gunCamera;

    [SerializeField] private float mainCamMaxTiltAngle = 90;

    /// <summary>
    /// Main Camera localEulerAngles.x (get and set)
    /// </summary>
    public float MainCamLocalEulerPitch
    {
        get => mainCamera.transform.localEulerAngles.x;
        set
        {
            Vector3 euler = mainCamera.transform.localEulerAngles;
            euler.x = value;
            euler.NormalizeAsEuler();

            euler.x = math.clamp(euler.x, -mainCamMaxTiltAngle, mainCamMaxTiltAngle);

            mainCamera.transform.localEulerAngles = euler;
        }
    }

    public void SetFOV(float fov)
    {
        mainCamera.fieldOfView = fov;
        gunCamera.fieldOfView = fov;
    }


    public void ZoomIn(float zoomMultiplier)
    {

    }

    /// <summary>
    /// Adds a rotation to main camera and returns the actual change in rotation as euler (after the <see cref="mainCamMaxTiltAngle"/>)
    /// </summary>
    public Vector3 AddRotationToMain(Vector3 toAddEuler)
    {
        Vector3 cEuler = mainCamera.transform.localEulerAngles;
        Vector3 newEuler = cEuler + toAddEuler;
        newEuler.NormalizeAsEuler();

        newEuler.x = math.clamp(newEuler.x, -mainCamMaxTiltAngle, mainCamMaxTiltAngle);

        mainCamera.transform.localEulerAngles = newEuler;

        return newEuler - cEuler;
    }
}