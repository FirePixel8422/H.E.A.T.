using UnityEngine;



public class GunRefHolder : MonoBehaviour
{
#pragma warning disable UDR0001
    public static GunRefHolder LocalInstance;
    public static GunRefHolder OpponentInstance;
#pragma warning restore UDR0001


    public Transform GunTransform { get; private set; }

    [Header("Part of the gun that glows based on heat")]
    [SerializeField] private Renderer emissionRendererObj;

    private Material emissionMatInstance;
    public Material EmissionMatInstance => emissionMatInstance;


    /// <summary>
    /// Setup ref sata and set static Instances
    /// </summary>
    public void Init(bool localClientIsOwner)
    {
        if (localClientIsOwner)
        {
            LocalInstance = this;
        }
        else
        {
            OpponentInstance = this;
        }

        GunTransform = transform;
        emissionMatInstance = emissionRendererObj.material;
    }

    /// <summary>
    /// Destroy Gun Gameobject
    /// </summary>
    public void DestroyGun()
    {
        Destroy(gameObject);
    }
}