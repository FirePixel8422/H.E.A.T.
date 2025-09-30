using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public class CameraHandler
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform mainCamTransform;
    [SerializeField] private Camera gunCamera;
    public Camera MainCamera => mainCamera;
    public Camera GunCamera => gunCamera;


    [SerializeField] private float baseFOV;
    private float currentFOV;

    private float sensitivityMultiplier = 1;



    [SerializeField] private float mainCamMaxTiltAngle = 90;

    /// <summary>
    /// Main Camera localEulerAngles.x (get and set)
    /// </summary>
    public float MainCamLocalEulerPitch
    {
        get => mainCamTransform.localEulerAngles.x;
        set
        {
            Vector3 euler = mainCamTransform.localEulerAngles;
            euler.x = math.clamp(NormalizeAngle(value), -mainCamMaxTiltAngle, mainCamMaxTiltAngle);

            mainCamTransform.localEulerAngles = euler;
        }
    }

    /// <summary>
    /// Normalize a single float angle into [-180, 180].
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        if (float.IsNaN(angle) || float.IsInfinity(angle))
            return 0f;

        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }

    public float GetADSSensitivityMultiplier()
    {
        float baseHalf = baseFOV * 0.5f * Mathf.Deg2Rad;
        float zoomHalf = currentFOV * 0.5f * Mathf.Deg2Rad;

        return math.tan(zoomHalf) / math.tan(baseHalf) * sensitivityMultiplier;
    }

    public void SetFOV(float fov)
    {
        baseFOV = fov;
        currentFOV = fov;
        mainCamera.fieldOfView = fov;
        gunCamera.fieldOfView = fov;
    }

    public void SetFOVZoomMultiplier(float fovMultiplier, float sensitivityMultiplier)
    {
        currentFOV = baseFOV * fovMultiplier;

        mainCamera.fieldOfView = currentFOV;
        gunCamera.fieldOfView = currentFOV;

        this.sensitivityMultiplier = sensitivityMultiplier;
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