using UnityEngine;



[CreateAssetMenu(fileName = "GunScoopeSO", menuName = "ScriptableObjects/Attachments/GunScope")]
public class GunScopeSO : GunAttachmentSO
{
    [Header("Stat modifications for gun bound to this attachment")]
    [SerializeField] private GunScopeStats stats;

    public override IGunAtachment Stats
    {
        get
        {
            stats.Type = AttachmentType.Scope;
            return stats;
        }
    }
}