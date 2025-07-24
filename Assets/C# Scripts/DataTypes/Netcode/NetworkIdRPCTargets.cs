using System.Collections.Generic;
using Unity.Netcode;



namespace FirePixel.Networking
{
    /// <summary>
    /// Custom struct to handle Network RPC Targets based on <see cref="NetworkManager"/> NetworkId System.
    /// </summary>
    [System.Serializable]
    public struct NetworkIdRPCTargets : INetworkSerializable
    {
        private bool sendToAll;
        private ulong[] targetNetworkIds;


        public NetworkIdRPCTargets(ulong[] targetNetworkIds)
        {
            sendToAll = false;
            this.targetNetworkIds = targetNetworkIds;
        }

        public static NetworkIdRPCTargets SendToAll()
        {
            return new NetworkIdRPCTargets
            {
                sendToAll = true
            };
        }

        public static NetworkIdRPCTargets SendToTargetClient(ulong clientNetworkId)
        {
            return new NetworkIdRPCTargets
            {
                targetNetworkIds = new ulong[] { clientNetworkId }
            };
        }

        public static NetworkIdRPCTargets SendToAllButClient(ulong clientNetworkId)
        {
            int playerCount = ClientManager.PlayerCount;
            IReadOnlyList<ulong> clientNetworkIds = NetworkManager.Singleton.ConnectedClientsIds;

            ulong[] targetClientIds = new ulong[playerCount - 1];
            int arrayIndex = 0;

            for (int i = 0; i < playerCount; i++)
            {
                if (clientNetworkIds[i] == clientNetworkId) continue;

                targetClientIds[arrayIndex++] = clientNetworkIds[i];
            }

            return new NetworkIdRPCTargets
            {
                targetNetworkIds = targetClientIds
            };
        }

        public static NetworkIdRPCTargets SendToAllButServer()
        {
            int playerCount = ClientManager.PlayerCount;
            IReadOnlyList<ulong> clientNetworkIds = NetworkManager.Singleton.ConnectedClientsIds;

            ulong[] targetClientIds = new ulong[playerCount - 1];
            int arrayIndex = 0;

            for (int i = 0; i < playerCount; i++)
            {
                if (clientNetworkIds[i] == 0) continue;

                targetClientIds[arrayIndex++] = clientNetworkIds[i];
            }

            return new NetworkIdRPCTargets
            {
                targetNetworkIds = targetClientIds
            };
        }

        /// <summary>
        /// Get if the local client is a target for the RPC. (Through <see cref="NetworkManager"/> networkId System
        /// </summary
        public bool IsTarget => CheckIsTarget();

        private bool CheckIsTarget()
        {
            if (sendToAll) return true;

            ulong clientNetworkId = NetworkManager.Singleton.LocalClientId;

            int idCount = targetNetworkIds.Length;
            for (int i = 0; i < idCount; i++)
            {
                if (targetNetworkIds[i] == clientNetworkId) return true;
            }

            return false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref sendToAll);
            serializer.SerializeValue(ref targetNetworkIds);
        }
    }
}