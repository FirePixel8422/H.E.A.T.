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
        else
        {
            RequestSync_ServerRPC(NetworkManager.LocalClientId);
        }

        SpawnPlayer_ServerRPC(NetworkManager.LocalClientId);
    }



    #region Spawn player and destroy all uneeded components on other clients side for your player

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer_ServerRPC(ulong ownerNetworkId)
    {
        NetworkObject spawnedPlayer = NetworkObject.InstantiateAndSpawn(playerPrefab, NetworkManager, ownerNetworkId);

        SetupPlayer_ClientRPC(spawnedPlayer.NetworkObjectId, ownerNetworkId);

        spawnedPlayerNetworkObjectIds[spawnedPlayerCount] = spawnedPlayer.NetworkObjectId;
        spawnedPlayerOwnerIds[spawnedPlayerCount] = ownerNetworkId;

        spawnedPlayerCount += 1;
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetupPlayer_ClientRPC(ulong networkObjectId, ulong ownerNetworkId)
    {
        if (NetworkManager.LocalClientId != ownerNetworkId)
        {
            NetworkObject playerObj = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];

            playerObj.GetComponent<ObjectDestroyer>().DestroyAll();
        }
    }

    #endregion


    [ServerRpc(RequireOwnership = false)]
    private void RequestSync_ServerRPC(ulong clientNetworkId)
    {
        SetupPlayers_ClientRPC(spawnedPlayerNetworkObjectIds, spawnedPlayerOwnerIds, NetworkIdRPCTargets.SendToTargetClient(clientNetworkId));
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetupPlayers_ClientRPC(ulong[] networkObjectId, ulong[] ownerNetworkId, NetworkIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget == false) return;

        for (int i = 0; i < spawnedPlayerCount; i++)
        {
            if (NetworkManager.LocalClientId != ownerNetworkId[i])
            {
                NetworkObject playerObj = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId[i]];

                playerObj.GetComponent<ObjectDestroyer>().DestroyAll();
            }
        }

    }
}