using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHUD : NetworkBehaviour
{
    public static PlayerHUD LocalInstance { get; private set; }


    [SerializeField] private Image crossHair;

    public void SetCrossHairAlpha(float alpha)
    {
        Color col = crossHair.color;
        col.a = alpha;

        crossHair.color = col;
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
    }
}
