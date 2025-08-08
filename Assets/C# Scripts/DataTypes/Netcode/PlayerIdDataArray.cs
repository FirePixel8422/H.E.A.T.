using Unity.Collections;
using Unity.Netcode;
using UnityEngine;



namespace FirePixel.Networking
{
    [System.Serializable]
    [Tooltip("Netcode friendly array to store client data (clientId (also called networkId), gameId (the Xth client this is in the lobby -1), > (SERVER ONLY) > username string and guid (unique player id string for LobbySystem)")]
    public struct PlayerIdDataArray : INetworkSerializable
    {
        [Header("[0] = 2? client with networkId 2 is client 0")]
        [SerializeField] private ulong[] networkIds;

        [Header("Total clients in server that are setup by game/team id system")]
        [SerializeField] private int playerCount;

        [Tooltip("Total clients in server that are setup by game/team id system")]
        public readonly int PlayerCount => playerCount;


        [SerializeField] private string[] usernamesOnServer;
        [SerializeField] private string[] guidsOnServer;


        public PlayerIdDataArray(int maxPlayerCount)
        {
            networkIds = new ulong[maxPlayerCount];

            usernamesOnServer = new string[maxPlayerCount];
            guidsOnServer = new string[maxPlayerCount];

            playerCount = 0;
        }


        #region Update Data

        public void AddPlayer(ulong addedNetworkId)
        {
            networkIds[playerCount] = addedNetworkId;
            playerCount += 1;
        }

        public void SetUserNameAndGUID(int playerGameId, string username, string guid)
        {
            usernamesOnServer[playerGameId] = username;
            guidsOnServer[playerGameId] = guid;
        }


        public void RemovePlayer(ulong removedNetworkId)
        {
            int removedGameId = GetPlayerGameId(removedNetworkId);

            playerCount -= 1;

            for (int i = removedGameId; i < playerCount; i++)
            {
                // Move down all the networkIds in the array by 1.
                networkIds[i] = networkIds[i + 1];
                usernamesOnServer[i] = usernamesOnServer[i + 1];
                guidsOnServer[i] = guidsOnServer[i + 1];
            }
        }

        #endregion


        #region Retrieve Data

        /// <summary>
        /// Get client gameId by converting that clients networkId (localPlayerId)
        /// </summary>
        /// <returns>The clients gameId</returns>
        public int GetPlayerGameId(ulong toConvertNetworkId)
        {
            //since dictionaries are not netcode friendly, there is just an networkId array, and the place in the array of the value "toConvertNetworkId" is the equivelent gameId
            for (int i = 0; i < playerCount; i++)
            {
                if (networkIds[i] == toConvertNetworkId)
                {
                    return i;
                }
            }


#if UNITY_EDITOR
            string errorString = "Cant Convert Id: " + toConvertNetworkId + ", networkIds are: ";
            for (int i = 0; i < playerCount; i++)
            {
                errorString += networkIds[i] + ", ";
            }

            DebugLogger.LogError(errorString);
#endif

            return -1;
        }

        public ulong GetPlayerNetworkId(int toConvertGameId)
        {
            return networkIds[toConvertGameId];
        }

        public string GetUserName(int toConvertGameId)
        {
            return usernamesOnServer[toConvertGameId].ToString();
        }

        public string GetPlayerGUID(int toConvertGameId)
        {
            return guidsOnServer[toConvertGameId].ToString();
        }

        #endregion



        // Method from unity netcode to serialize this struct (PlayerIdDataArray) before sending it through a network function (RPC) so it doesnt throw an error
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref networkIds);
            serializer.SerializeValue(ref playerCount);
        }
    }
}