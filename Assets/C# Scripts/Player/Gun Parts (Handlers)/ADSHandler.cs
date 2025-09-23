using UnityEngine;



[System.Serializable]
public class ADSHandler
{
    [Header("Maps Scope Multpliers (Time, X1 X2 X4) to a FOV Multiplier (Value)")]
    [SerializeField] private NativeSampledAnimationCurve fovMultiplierCurve = NativeSampledAnimationCurve.Default;

    [SerializeField] private Animator anim;
    private CameraHandler camHandler;

    public GunADSStats stats;
    public float ZoomedInPercent => zoomInTransitionPercent;


    private float prevFovMultiplier = -1;

    private float zoomInTransitionPercent;
    private bool isZoomingIn;


    public void Init(CameraHandler camHandler)
    {
        this.camHandler = camHandler;
        fovMultiplierCurve.Bake();
    }

    public void OnZoomInput(bool performed)
    {
        isZoomingIn = performed;

        //COnvert string to animator HAshes????
        //COnvert string to animator HAshes????
        //COnvert string to animator HAshes????
        //COnvert string to animator HAshes????
        anim.CrossFadeInFixedTime(performed ? stats.zoomAnimationName : stats.normalAnimationName, stats.zoomInTime, stats.animLayer);
    }

    public void SwapGun()
    {
        anim.PlayInFixedTime(stats.normalAnimationName, stats.animLayer);
    }

    public void OnUpdate(float deltaTime)
    {
        float fovMultiplier;

        if (isZoomingIn)
        {
            zoomInTransitionPercent = Mathf.MoveTowards(zoomInTransitionPercent, 1, 1 / stats.zoomInTime * deltaTime);

            fovMultiplier = fovMultiplierCurve.Evaluate(zoomInTransitionPercent * stats.zoomMultiplier);
        }
        else
        {
            zoomInTransitionPercent = Mathf.MoveTowards(zoomInTransitionPercent, 0, 1 / stats.zoomOutTime * deltaTime);

            fovMultiplier = fovMultiplierCurve.Evaluate(zoomInTransitionPercent * stats.zoomMultiplier);
        }

        if (fovMultiplier != prevFovMultiplier)
        {
            camHandler.SetFOVZoomMultiplier(fovMultiplier);
            PlayerHUD.LocalInstance.SetCrossHairAlpha(1 - zoomInTransitionPercent);
        }
        prevFovMultiplier = fovMultiplier;
    }


    public void Dispose()
    {
        fovMultiplierCurve.Dispose();
    }


#if UNITY_EDITOR

    [ContextMenu("DEBUG Rebake fov Curve")]
    public void DEBUGRebakeCurve()
    {
        fovMultiplierCurve.Bake();
    }

#endif
}