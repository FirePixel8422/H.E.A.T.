using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class HeatSinkHandler
{
    [SerializeField] private Image heatBar;
    [SerializeField] private Animator anim;

    public GunHeatSink[] gunHeatSinks;

    public void Init(int gunCount)
    {
        gunHeatSinks = new GunHeatSink[gunCount];
        for (int i = 0; i < gunCount; i++)
        {
            gunHeatSinks[i].Init(heatBar, anim);
        }
    }
}
