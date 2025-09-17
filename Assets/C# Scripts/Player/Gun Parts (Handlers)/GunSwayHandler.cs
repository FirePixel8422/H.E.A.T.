using UnityEngine;



[System.Serializable]
public class GunSwayHandler
{
    [SerializeField] private Transform swayTransform;
    [SerializeField] private GunSwayStats stats;

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

    public void OnUpdate()
    {

    }

    public void Dispose()
    {
        stats.Dispose();
    }
}