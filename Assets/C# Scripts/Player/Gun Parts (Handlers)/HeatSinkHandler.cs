using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class HeatSinkHandler
{
    [SerializeField] private Image heatBar;
    [SerializeField] private Animator anim;

    public GunHeatSink[] gunHeatSinks;

    public GunHeatSink this[int index]
    {
        get => gunHeatSinks[index];
        set => gunHeatSinks[index] = value;
    }

    /// <summary>
    /// Create and setup heatSink array for every gun through GunManager
    /// </summary>
    public void Init()
    {
        GunManager.Instance.SetupHeatSinks(out gunHeatSinks, heatBar, anim);
    }

    public void OnSwapGun(int newGunId)
    {
        // Manual update on gun swap of heatBarUI and Animation update
        heatBar.fillAmount = gunHeatSinks[newGunId].HeatPercentage;
        anim.SetBool("Overheated", gunHeatSinks[newGunId].Overheated);
    }
}
