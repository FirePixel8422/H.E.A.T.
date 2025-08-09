using FirePixel.Networking;
using Unity.Netcode;
using UnityEngine;


public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    // Only server has this data
    private ulong[] spawnedPlayerNetworkObjectIds;
    private ulong[] spawnedPlayerOwnerIds;
    private int spawnedPlayerCount;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            spawnedPlayerNetworkObjectIds = new ulong[GlobalGameSettings.MaxPlayers];
            spawnedPlayerOwnerIds = new ulong[GlobalGameSettings.MaxPlayers];
        }

        SpawnPlayer_ServerRPC(NetworkManager.LocalClientId);
    }



    #region Spawn player and destroy all uneeded components on other clients side for your player

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SpawnPlayer_ServerRPC(ulong ownerNetworkId)
    {
        NetworkObject spawnedPlayer = NetworkObject.InstantiateAndSpawn(playerPrefab, NetworkManager, ownerNetworkId);

        spawnedPlayerNetworkObjectIds[spawnedPlayerCount] = spawnedPlayer.NetworkObjectId;
        spawnedPlayerOwnerIds[spawnedPlayerCount] = ownerNetworkId;

        spawnedPlayerCount += 1;
    }

    #endregion
}