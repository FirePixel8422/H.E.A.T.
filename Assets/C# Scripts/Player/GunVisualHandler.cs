using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;


public class GunVisualHandler : NetworkBehaviour
{
    [SerializeField] private HDRGradient heatGradient;
    [SerializeField] private Renderer toUseMat;

    private Material mat;
    private int emissionId;


    private void Start()
    {
        mat = toUseMat.sharedMaterial;
        emissionId = Shader.PropertyToID("_EmissionColor");
    }

    public void UpdateHeatEmission(float percent)
    {
        SetHeatEmission(percent);
        DebugLogger.Log(percent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetHeatEmission(float percent)
    {
        Color heatColor = heatGradient.gradient.Evaluate(percent);
        mat.SetColor(emissionId, heatColor);
    }


#if UNITY_EDITOR

    [Range(0, 1)]
    [SerializeField] private float DEBUG_HeatPercentage;

    private void OnValidate()
    {
        Color heatColor = heatGradient.gradient.Evaluate(DEBUG_HeatPercentage);
        toUseMat.sharedMaterial.SetColor("_EmissionColor", heatColor);
    }

#endif
}
