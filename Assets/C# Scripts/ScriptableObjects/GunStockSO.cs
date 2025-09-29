using UnityEngine;



[CreateAssetMenu(fileName = "GunStockSO", menuName = "ScriptableObjects/Attachments/GunStock")]
public class GunStockSO : GunAttachmentSO
{
    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunStockStats stats;

    public override IGunAtachment Stats
    {
        get
        {
            stats.Type = AttachmentType.Stock;
            return stats;
        }
    }
}