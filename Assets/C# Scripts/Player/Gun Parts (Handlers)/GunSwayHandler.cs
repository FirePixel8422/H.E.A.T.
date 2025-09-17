using UnityEngine;



[System.Serializable]
public class GunSwayHandler
{
    [SerializeField] private Transform swayTransform;
    public GunSwayStats stats;

    /// <summary>
    /// MovementState affects sway, speed percent01 of maxSpeed and IsAirborne are used for sway percentage
    /// </summary>
    public float GetSpreadMultiplierNerf(float percentage)
    {
        return stats.spreadMultplierCurve.Evaluate(percentage);
    }


    public void SwapGun(GunSwayStats stats)
    {
        stats.Dispose();

        this.stats = stats;
        stats.BakeAllCurves();
    }

    /// <summary>
    /// Updated through PlayerController when the mouse is moved (moving the camera)
    /// </summary>
    public void OnCameraUpdate()
    {

    }
    /// <summary>
    /// Updated every FixedUpdate frame after player movement
    /// </summary>
    public void OnMovementUpdate(float speedPercentage, bool isGrounded)
    {

    }

    public void Dispose()
    {
        stats.Dispose();
    }
}