using UnityEngine;


[System.Serializable]
public class GunVisualHandler
{
    [SerializeField] private HDRGradient heatGradient;

    private Material matInstance;
    private int emissionId;


    public void Init(Material _matInstance)
    {
        matInstance = _matInstance;
        emissionId = Shader.PropertyToID("_EmissionColor");
    }

    public void UpdateHeatEmission(float percent)
    {
        Color heatColor = heatGradient.gradient.Evaluate(percent);
        matInstance.SetColor(emissionId, heatColor);
    }
}
