using UnityEngine;



[CreateAssetMenu(fileName = "GunStockSO", menuName = "ScriptableObjects/Attachments/GunStock")]
public class GunStockSO : GunAttachmentSO
{
    [Header("Type of attachment")]
    [SerializeField] private AttachmentType type = AttachmentType.Stock;

    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunStockStats stats;


    public override AttachmentType Type => type;
    public override IGunAtachment Stats => stats;
}