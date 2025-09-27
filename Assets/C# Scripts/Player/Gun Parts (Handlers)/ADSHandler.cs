using UnityEngine;


[System.Serializable]
public class ADSHandler
{
    [Header("Maps Scope Multpliers (Time, X1 X2 X4) to a FOV Multiplier (Value)")]
    [SerializeField] private NativeSampledAnimationCurve fovMultiplierCurve = NativeSampledAnimationCurve.Default;

    [SerializeField] private Animator anim;

    public GunADSStats stats;
    public float ZoomedInPercent => zoomInTransitionValue / stats.zoomMultiplier;


    private CameraHandler camHandler;

    private float zoomInTransitionValue;
    private bool isZoomingIn;

    private float prevFovMultiplier = -1;

    private int normalAnimationHash;
    private int zoomAnimationHash;


    public void Init(CameraHandler camHandler)
    {
        this.camHandler = camHandler;
        fovMultiplierCurve.Bake();
    }

    public void OnZoomInput(bool performed)
    {
        // Only continue on value changed
        if (performed != isZoomingIn)
        {
            anim.CrossFadeInFixedTime(performed ? zoomAnimationHash : normalAnimationHash, stats.zoomInTime, stats.animLayer);

            isZoomingIn = performed;
        }
    }

    public void OnSwapGun()
    {
        stats.GetAnimationHashes(out normalAnimationHash, out zoomAnimationHash);

        anim.PlayInFixedTime(normalAnimationHash, stats.animLayer);
        isZoomingIn = false;
    }

    public void OnUpdate(float deltaTime)
    {
        float fovMultiplier;

        if (isZoomingIn)
        {
            zoomInTransitionValue = Mathf.MoveTowards(zoomInTransitionValue, stats.zoomMultiplier, 1 / stats.zoomInTime * stats.zoomMultiplier * deltaTime);

            fovMultiplier = fovMultiplierCurve.Evaluate(zoomInTransitionValue);
        }
        else
        {
            zoomInTransitionValue = Mathf.MoveTowards(zoomInTransitionValue, 0, 1 / stats.zoomOutTime * stats.zoomMultiplier * deltaTime);

            fovMultiplier = fovMultiplierCurve.Evaluate(zoomInTransitionValue);
        }

        if (fovMultiplier != prevFovMultiplier)
        {
            float sensitivityMultiplier = Mathf.Lerp(1, stats.adsSensitivityMultiplier, ZoomedInPercent);

            camHandler.SetFOVZoomMultiplier(fovMultiplier, sensitivityMultiplier);
            PlayerHUD.LocalInstance.SetCrossHairAlpha(1 - zoomInTransitionValue);
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