using Unity.Netcode;


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
}