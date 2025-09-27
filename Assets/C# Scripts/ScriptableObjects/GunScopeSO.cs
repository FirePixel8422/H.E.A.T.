using UnityEngine;



[CreateAssetMenu(fileName = "GunScoopeSO", menuName = "ScriptableObjects/Attachments/GunScope")]
public class GunScopeSO : GunAttachmentSO
{
    [Header("Type of attachment")]
    [SerializeField] private AttachmentType type = AttachmentType.Scope;

    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunScopeStats stats;


    public override AttachmentType Type => type;
    public override IGunAtachment Stats => stats;
}