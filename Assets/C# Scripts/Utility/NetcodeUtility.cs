using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Utility class for handling Netcode operations, such as sending RPCs to clients.
/// </summary>
public static class NetcodeUtility
{
    /// <summary>
    /// Send RPC to clientGameId
    /// </summary>
    public static ClientRpcParams SendToClient(int clientGameId)
    {
        ulong clientNetworkId = ClientManager.GetClientNetworkId(clientGameId);
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientNetworkId }
            }
        };
    }

    /// <summary>
    /// Send RPC to enemy of clientGameId (Only works in 1v1 situations)
    /// </summary>
    public static ClientRpcParams SendToOppositeClient(int clientGameId)
    {
        int opponentGameId = (clientGameId == 0) ? 1 : 0;
        ulong opponentNetworkId = ClientManager.GetClientNetworkId(opponentGameId);
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { opponentNetworkId }
            }
        };
    }

    /// <summary>
    /// Send RPC to all clients except the one with clientGameId
    /// </summary>
    public static ClientRpcParams SendToAllButClient(int clientGameId)
    {
        int playerCount = ClientManager.PlayerCount;
        ulong[] targetClientIds = new ulong[playerCount - 1];

        int arrayIndex = 0;
        for (int i = 0; i < playerCount; i++)
        {
            if (i == clientGameId) continue; // Skip the client we don't want to send to

            targetClientIds[arrayIndex] = ClientManager.GetClientNetworkId(i);
            arrayIndex += 1;
        }

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClientIds
            }
        };
    }

    /// <summary>
    /// Send RPC to all clients except the one that is also the host
    /// </summary>
    public static ClientRpcParams SendToAllButHost()
    {
        int playerCount = ClientManager.PlayerCount;
        ulong[] targetClientIds = new ulong[playerCount - 1];

        int arrayIndex = 0;
        for (int i = 0; i < playerCount; i++)
        {
            if (i == 0) continue; // Skip the client we don't want to send to

            targetClientIds[arrayIndex] = ClientManager.GetClientNetworkId(i);
            arrayIndex += 1;
        }

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClientIds
            }
        };
    }


    /// <summary>
    /// Send RPC to clientNetworkId
    /// </summary>
    public static ClientRpcParams SendToClient(ulong clientNetworkId)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientNetworkId }
            }
        };
    }

    /// <summary>
    /// Send RPC to all clients except the one with clientNetworkId
    /// </summary>
    public static ClientRpcParams SendToAllButClient(ulong clientNetworkId)
    {
        int playerCount = ClientManager.PlayerCount;

        ulong[] clientNetworkIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();
        ulong[] targetClientIds = new ulong[playerCount - 1];

        int arrayIndex = 0;
        for (int i = 0; i < playerCount; i++)
        {
            if (clientNetworkIds[i] == clientNetworkId) continue; // Skip the client we don't want to send to

            targetClientIds[arrayIndex] = clientNetworkIds[i];
            arrayIndex += 1;
        }

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClientIds
            }
        };
    }
}
