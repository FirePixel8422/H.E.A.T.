using UnityEngine;



[CreateAssetMenu(fileName = "GunGripSO", menuName = "ScriptableObjects/Attachments/GunGrip")]
public class GunGripSO : GunAttachmentSO
{
    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunGripStats stats;

    public override IGunAtachment Stats
    {
        get
        {
            stats.Type = AttachmentType.Grip;
            return stats;
        }
    }
}