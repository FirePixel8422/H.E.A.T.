using UnityEngine;



[CreateAssetMenu(fileName = "GunScoopeSO", menuName = "ScriptableObjects/Attachments/GunBattery")]
public class GunBatterySO : GunAttachmentSO
{
    [Header("Type of attachment")]
    [SerializeField] private AttachmentType type = AttachmentType.Battery;

    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunBatteryStats stats;


    public override AttachmentType Type => type;
    public override IGunAtachment Stats => stats;
}