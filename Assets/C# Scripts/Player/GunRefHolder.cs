using UnityEngine;



public class GunRefHolder : MonoBehaviour
{
    public Camera ScopeCamera { get; set; }

    [SerializeField] private Transform muzzleFlashPoint;
    public TransformOffset MuzzleTransformOffset;

    public Vector3 MuzzleFlashPosition => muzzleFlashPoint.position;
    public Vector3 MuzzleFlashScale => muzzleFlashPoint.localScale;


    //[Header("Part of the gun that glows based on heat")]
    //[SerializeField] private Renderer emissionRendererObj;

    //public Material EmissionMatInstance { get; private set; }


    public void Init()
    {
        //EmissionMatInstance = emissionRendererObj.material;

        ScopeCamera = GetComponentInChildren<Camera>(true);
    }

    /// <summary>
    /// Spawn target attachment under the gun with configured offset in AttchmentSO Data
    /// </summary>
    public void SpawnAttachment(GunAttachmentSO attachment)
    {
        GameObject attachmentObj = Instantiate(attachment.AttachmentPrefab, transform);

        attachmentObj.transform.SetLocalPositionAndRotation(attachment.TransformOffset.position, attachment.TransformOffset.Rotation);
        attachmentObj.transform.localScale = attachment.TransformOffset.scale;

        attachment.Stats.ApplyToGunObject(this);

        muzzleFlashPoint.SetLocalPositionAndRotation(muzzleFlashPoint.localPosition + MuzzleTransformOffset.position, muzzleFlashPoint.localRotation * MuzzleTransformOffset.Rotation);
        muzzleFlashPoint.localScale = Vector3.Scale(muzzleFlashPoint.localScale, MuzzleTransformOffset.scale);
    }

    /// <summary>
    /// Destroy Gun Gameobject
    /// </summary>
    public void DestroyGun()
    {
        Destroy(gameObject);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(MuzzleFlashPosition, MuzzleFlashScale * 0.025f);
        Gizmos.DrawWireCube(MuzzleFlashPosition + transform.forward * 0.025f, MuzzleFlashScale * 0.01f);
    }
}