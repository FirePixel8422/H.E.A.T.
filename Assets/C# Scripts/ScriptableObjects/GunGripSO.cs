using UnityEngine;



[CreateAssetMenu(fileName = "GunGripSO", menuName = "ScriptableObjects/Attachments/GunGrip")]
public class GunGripSO : GunAttachmentSO
{
    [Header("Type of attachment")]
    [SerializeField] private AttachmentType type = AttachmentType.Grip;

    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunGripStats stats;


    public override AttachmentType Type => type;
    public override IGunAtachment Stats => stats;
}