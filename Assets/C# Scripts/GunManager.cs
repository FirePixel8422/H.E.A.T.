using System;
using Unity.VisualScripting;
using UnityEngine;


public class GunManager : MonoBehaviour
{
    public static GunManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        SetupAttachments();
    }


    [SerializeField] private GunAttachmentSO[] globalAttachmentsList;

    [SerializeField] private GunSO[] baseGuns;

    [SerializeField] private ArrayWrapper<int>[] attachmentIdsList;
    [SerializeField] private CompleteGunStatsSet[] currentGunStats;

    public int GunCount => baseGuns.Length;

    private int currentGunId;


    private void SetupAttachments()
    {
        int attachmentCount = globalAttachmentsList.Length;
        for (int i = 0; i < attachmentCount; i++)
        {
            globalAttachmentsList[i].Stats.AttachmentId = i;
        }

        int gunCount = baseGuns.Length;
        attachmentIdsList = new ArrayWrapper<int>[gunCount];
        
        for (int i = 0; i < gunCount; i++)
        {
            attachmentIdsList[i].Value = new int[5];

            Array.Fill(attachmentIdsList[i].Value, -1);
        }

        currentGunStats = new CompleteGunStatsSet[gunCount];

        CalculateGunStats();
    }

    public void CalculateGunStats()
    {
        int gunCount = baseGuns.Length;
        for (int gunId = 0; gunId < gunCount; gunId++)
        {
            GunSO targetGun = baseGuns[gunId];

            CompleteGunStatsSet targetStatsSet = targetGun.BaseStats;

            int attachmentCount = targetGun.BaseAttachmentsCount;
            for (int i = 0; i < attachmentCount; i++)
            {
                IGunAtachment targetAttachment = targetGun.BaseAttachments[i].Stats;

                targetAttachment.ApplyToBaseStats(ref targetStatsSet);

                attachmentIdsList[gunId].Value[i] = targetAttachment.AttachmentId;
            }

            targetStatsSet.BakeAllCurves();

            currentGunStats[gunId] = targetStatsSet;
        }
    }

    public void SetupHeatSinks(out GunHeatSink[] heatSinks, UnityEngine.UI.Image heatBar, Animator anim)
    {
        heatSinks = new GunHeatSink[GunCount];
        for (int i = 0; i < GunCount; i++)
        {
            heatSinks[i] = new GunHeatSink();
            heatSinks[i].stats = currentGunStats[i].heatSinkStats;
            heatSinks[i].Init(heatBar, anim);
        }
    }

    /// <summary>
    /// Swap gun and get baseGunstats by gunId.
    /// </summary>
    public void SwapGun(
        Transform gunParentTransform, int gunId, ref GunRefHolder gunRefHolder,
        out GunCoreStats coreStats,
        out GunAudioStats audioStats,
        out GunShakeStats shakeStats,
        out GunSwayStats swayStats,
        out GunADSStats gunADSStats)
    {
        currentGunId = gunId;

        if (gunRefHolder != null)
        {
            gunRefHolder.DestroyGun();
        }

        gunRefHolder = Instantiate(baseGuns[gunId].GunPrefab, gunParentTransform);
        gunRefHolder.OnSwapGun();

        for (int i = 0; i < 5; i++)
        {
            int attachmentId = attachmentIdsList[gunId][i];

            if (attachmentId == -1 || attachmentId >= globalAttachmentsList.Length) continue;
                        
            gunRefHolder.SpawnAttachment(globalAttachmentsList[attachmentId]);
        }

        currentGunStats[gunId].GetStatsCopy(out coreStats, out audioStats, out _, out shakeStats, out swayStats, out gunADSStats);
    }

    public int GetNextGunId() 
    {
        currentGunId = (currentGunId + 1) % baseGuns.Length;
        return currentGunId;
    }


    public string GetCurrentGunName() => baseGuns[currentGunId].name;


    private void OnDestroy()
    {
        int gunCount = baseGuns.Length;
        for (int gunId = 0; gunId < gunCount; gunId++)
        {
            baseGuns[gunId].BaseStats.Dispose();
        }
    }
}
