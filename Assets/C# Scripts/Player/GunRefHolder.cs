using UnityEngine;



public class GunRefHolder : MonoBehaviour
{
    [Header("Part of the gun that glows based on heat")]
    [SerializeField] private Renderer emissionRendererObj;

    public Material EmissionMatInstance { get; private set; }


    public void OnSwapGun()
    {
        EmissionMatInstance = emissionRendererObj.material;
    }

    /// <summary>
    /// Spawn target attachment under the gun with configured offset in AttchmentSO Data
    /// </summary>
    public void SpawnAttachment(GunAttachmentSO attachment)
    {
        GameObject attachmentObj = Instantiate(attachment.AttachmentPrefab, transform);

        attachmentObj.transform.SetLocalPositionAndRotation(attachment.TransformOffset.position, attachment.TransformOffset.Rotation);
        attachmentObj.transform.localScale = attachment.TransformOffset.scale;
    }

    /// <summary>
    /// Destroy Gun Gameobject
    /// </summary>
    public void DestroyGun()
    {
        Destroy(gameObject);
    }
}