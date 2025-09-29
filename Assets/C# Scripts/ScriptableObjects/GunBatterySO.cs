using UnityEngine;



[CreateAssetMenu(fileName = "GunScoopeSO", menuName = "ScriptableObjects/Attachments/GunBattery")]
public class GunBatterySO : GunAttachmentSO
{
    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunBatteryStats stats;

    public override IGunAtachment Stats
    {
        get
        {
            stats.Type = AttachmentType.Battery;
            return stats;
        }
    }
}