using UnityEngine;



[CreateAssetMenu(fileName = "GunMuzzleSO", menuName = "ScriptableObjects/Attachments/GunMuzzle")]
public class GunMuzzleSO : GunAttachmentSO
{
    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunMuzzleStats stats;

    public override IGunAtachment Stats
    {
        get
        {
            stats.Type = AttachmentType.Muzzle;
            return stats;
        }
    }
}