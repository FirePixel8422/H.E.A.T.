using Unity.Netcode;



namespace FirePixel.Networking
{
    /// <summary>
    /// Custom struct to handle Network RPC Targets based on <see cref="ClientManager"/> GameId System.
    /// </summary>
    [System.Serializable]
    public struct GameIdRPCTargets : INetworkSerializable
    {
        public bool sendToAll;
        public int[] targetGameIds;


        public GameIdRPCTargets(int[] targetGameIds)
        {
            sendToAll = false;
            this.targetGameIds = targetGameIds;
        }

        public static GameIdRPCTargets SendToAll()
        {
            return new GameIdRPCTargets
            {
                sendToAll = true
            };
        }

        public static GameIdRPCTargets SendToTargetClient(int clientGameId)
        {
            return new GameIdRPCTargets
            {
                targetGameIds = new int[] { clientGameId }
            };
        }

        public static GameIdRPCTargets SendToOppositeClient(int clientGameId)
        {
            int opponentGameId = (clientGameId == 0) ? 1 : 0;

            return new GameIdRPCTargets
            {
                targetGameIds = new int[] { opponentGameId }
            };
        }

        public static GameIdRPCTargets SendToAllButClient(int clientGameId)
        {
            int playerCount = ClientManager.PlayerCount;
            int[] targetClientIds = new int[playerCount - 1];

            int arrayIndex = 0;
            for (int i = 0; i < playerCount; i++)
            {
                if (i == clientGameId) continue;

                targetClientIds[arrayIndex++] = i;
            }

            return new GameIdRPCTargets
            {
                targetGameIds = targetClientIds
            };
        }

        public static GameIdRPCTargets SendToAllButServer()
        {
            int playerCount = ClientManager.PlayerCount;
            int[] targetClientIds = new int[playerCount - 1];

            int arrayIndex = 0;
            for (int i = 0; i < playerCount; i++)
            {
                if (i == 0) continue;

                targetClientIds[arrayIndex++] = i;
            }

            return new GameIdRPCTargets
            {
                targetGameIds = targetClientIds
            };
        }

        /// <summary>
        /// Get if the local client is a target for the RPC. (Through Custom <see cref="ClientManager"/> GameId System
        /// </summary
        public bool IsTarget => CheckIsTarget();

        public bool CheckIsTarget()
        {
            if (sendToAll) return true;

            int clientGameId = ClientManager.LocalClientGameId;

            int idCount = targetGameIds.Length;
            for (int i = 0; i < idCount; i++)
            {
                if (targetGameIds[i] == clientGameId) return true;
            }

            return false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref sendToAll);
            if (sendToAll == false)
            {
                serializer.SerializeValue(ref targetGameIds);
            }
        }
    }
}