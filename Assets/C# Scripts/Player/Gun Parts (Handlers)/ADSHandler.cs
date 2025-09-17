using UnityEngine;



[System.Serializable]
public class ADSHandler
{
    [Header("Maps Scope Multpliers (Time, X1 X2 X4) to a FOV Multiplier (Value)")]
    [SerializeField] private NativeSampledAnimationCurve fovMultplierCurve = NativeSampledAnimationCurve.Default;

    private CameraHandler camHandler;
    private float fovMultiplier;


    public void SetNewScopeMultiplier(float multiplier)
    {
        float fovMultiplier = fovMultplierCurve.Evaluate(multiplier);
    }


    public void Init(CameraHandler camHandler)
    {
        this.camHandler = camHandler;
    }


    public void Dispose()
    {
        fovMultplierCurve.Dispose();
    }
}