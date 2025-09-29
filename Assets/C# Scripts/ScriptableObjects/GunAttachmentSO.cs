using UnityEngine;


public class GunAttachmentSO : ScriptableObject
{
    [Space(20)]
    [SerializeField] private GameObject attachmentPrefab;
    public GameObject AttachmentPrefab => attachmentPrefab;


    [SerializeField] private LocalOffset transformOffset;
    public LocalOffset TransformOffset => transformOffset;


    public virtual IGunAtachment Stats
    {
        get
        {
            DebugLogger.LogError("Null Attachment is used");
            return default;
        }
    }
}