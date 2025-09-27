using UnityEngine;


public class GunAttachmentSO : ScriptableObject
{
    [Header("Actual attachment (Prefab)")]
    [SerializeField] private GameObject attachmentPrefab;
    public GameObject AttachmentPrefab => attachmentPrefab;


    [SerializeField] private LocalOffset transformOffset;
    public LocalOffset TransformOffset => transformOffset;


    public virtual AttachmentType Type
    {
        get
        {
            DebugLogger.LogError("Null Attachment is used");
            return default;
        }
    }
    public virtual IGunAtachment Stats
    {
        get
        {
            DebugLogger.LogError("Null Attachment is used");
            return default;
        }
    }
}