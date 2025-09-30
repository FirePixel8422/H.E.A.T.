using Unity.Netcode;


namespace FirePixel.Networking
{
    [System.Serializable]
    public struct MatchSettings : INetworkSerializable
    {
        public int GetSavedInt(int id)
        {
            return id switch
            {
                0 => privateLobby ? 1 : 0,
                _ => -1,
            };
        }
        public void SetIntData(int id, int value)
        {
            switch (id)
            {
                case 0:
                    privateLobby = value == 1;
                    break;
                default:
#if UNITY_EDITOR
                    DebugLogger.LogError("Error asigning value in MatchSettings.cs");
#endif
                    break;
            }
        }


        public bool privateLobby;


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref privateLobby);
        }
    }
}