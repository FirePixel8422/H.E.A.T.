using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public static class ResolutionScaler
{
    public static float renderScale;
    public static UniversalRenderPipelineAsset urp;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        renderScale = 0.05f;

        urp.renderScale = renderScale;
    }


    public static void SetRenderScale(float newRenderScale)
    {
        if (newRenderScale != renderScale)
        {
            renderScale = newRenderScale;

            urp.renderScale = newRenderScale;
        }
    }
}