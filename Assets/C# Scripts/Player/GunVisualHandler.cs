using UnityEngine;


[System.Serializable]
public class GunVisualHandler
{
    [SerializeField] private HDRGradient heatGradient;

    [SerializeField] private Material emissionMatInstance;
    private int emissionId;


    public void Init()
    {
        emissionId = Shader.PropertyToID("_EmissionColor");
    }

    public void SwapGun(Material _matInstance)
    {
        emissionMatInstance = _matInstance;
    }

    public void UpdateHeatEmission(float percent)
    {
        if (emissionMatInstance == null) return;

        Color heatColor = heatGradient.gradient.Evaluate(percent);
        emissionMatInstance.SetColor(emissionId, heatColor);
    }
}
