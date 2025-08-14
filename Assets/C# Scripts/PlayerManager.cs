using FirePixel.Networking;
using Unity.Netcode;
using UnityEngine;


public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;


    public override void OnNetworkSpawn()
    {
        SpawnPlayer_ServerRPC(NetworkManager.LocalClientId);
    }


    /// <summary>
    /// Spawn player and destroy all unneeded components on other clients side for your player
    /// </summary>

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SpawnPlayer_ServerRPC(ulong ownerNetworkId)
    {
        NetworkObject spawnedPlayer = NetworkObject.InstantiateAndSpawn(playerPrefab, NetworkManager, ownerNetworkId);
    }
}