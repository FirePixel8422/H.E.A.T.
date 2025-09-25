using UnityEngine;


[System.Serializable]
public struct GunScopeStats : IGunAtachment
{
    [SerializeField] private SmartAttributeFloat zoomMultiplier;

    public void ApplyToBaseStats(GunManager gunManager, int gunId)
    {
        //zoomMultiplier.ApplyToStat(ref adsStats.zoomMultiplier);
    }
}