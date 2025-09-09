#if UNITY_EDITOR
using Unity.Mathematics;


[System.Serializable]
public struct DebugRecoilPatternData
{
    public string weaponName;
    public float2[] recoilPoints;

    public DebugRecoilPatternData(string weaponName, float2[] recoilPoints)
    {
        this.weaponName = weaponName;
        this.recoilPoints = recoilPoints;
    }
}
#endif