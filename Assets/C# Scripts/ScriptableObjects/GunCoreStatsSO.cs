using UnityEngine;



[CreateAssetMenu(fileName = "GunCoreStatsSO", menuName = "ScriptableObjects/GunCore Stats")]
public class GunCoreStatsSO : ScriptableObject
{
    public GunCoreStats stats = GunCoreStats.Default;
}
