using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHUD : NetworkBehaviour
{
    public static PlayerHUD LocalInstance { get; private set; }


    [SerializeField] private Image crossHair;
    private Color crossHairColor;

    public void SetCrossHairAlpha(float alpha)
    {
        crossHairColor.a = alpha;

        crossHair.color = crossHairColor;
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            crossHairColor = crossHair.color;
        }
    }


#if UNITY_EDITOR
    private void Start()
    {
        if (transform.TryFindObjectOfType(out NetworkManager _, true) == false)
        {
            LocalInstance = this;
        }
    }
#endif
}
