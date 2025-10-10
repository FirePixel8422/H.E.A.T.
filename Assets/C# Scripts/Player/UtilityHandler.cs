using Unity.Netcode;
using UnityEngine;




public class UtilityHandler : NetworkBehaviour
{
    public UtilityStats[] utility;

    public void UseUtility(int id)
    {
        if (utility[id].IsUsable)
        {
            utility[id].Use();

            SpawnUseUtility_ServerRPC();
        }
    }


    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SpawnUseUtility_ServerRPC()
    {
        SyncUseUtility_ClientRPC();
    }
    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SyncUseUtility_ClientRPC()
    {

    }
}