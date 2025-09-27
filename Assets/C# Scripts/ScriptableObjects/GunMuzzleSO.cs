using UnityEngine;



[CreateAssetMenu(fileName = "GunMuzzleSO", menuName = "ScriptableObjects/Attachments/GunMuzzle")]
public class GunMuzzleSO : GunAttachmentSO
{
    [Header("Type of attachment")]
    [SerializeField] private AttachmentType type = AttachmentType.Muzzle;

    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunMuzzleStats stats;


    public override AttachmentType Type => type;
    public override IGunAtachment Stats => stats;
}