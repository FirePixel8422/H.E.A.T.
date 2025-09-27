using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "GunCoreStatsSO", menuName = "ScriptableObjects/Gun")]
public class GunSO : ScriptableObject
{
    [Header("Actual gun (Prefab)")]
    [SerializeField] private GunRefHolder gunPrefab;
    public GunRefHolder GunPrefab => gunPrefab;


    [Header("base stats bound to this gun")]
    [SerializeField] private CompleteGunStatsSet baseStats = CompleteGunStatsSet.Default;
    public CompleteGunStatsSet BaseStats => baseStats;


    [Header("Base attachments on this gun")]
    [SerializeField] private GunAttachmentSO[] baseAttachments;
    public GunAttachmentSO[] BaseAttachments => baseAttachments;

    /// <summary>
    /// Built in Array null check
    /// </summary>
    public int BaseAttachmentsCount => baseAttachments == null ? 0 : baseAttachments.Length;


#if UNITY_EDITOR
    public void SetGunStatsADSRecoilPattern(float2[] pattern)
    {
        baseStats.coreStats.adsRecoilPattern = pattern;
    }

    public GunCoreStats GetGunCoreStats() => baseStats.coreStats;

    private void OnValidate()
    {
        if (BaseAttachmentsCount > 5)
        {
            GunAttachmentSO[] prev = baseAttachments;

            baseAttachments = new GunAttachmentSO[5];
            for (int i = 0; i < 5; i++)
            {
                baseAttachments[i] = prev[i];
            }

            DebugLogger.LogError("Only 5 attachments per gun are supported!");
        }
    }
#endif
}
