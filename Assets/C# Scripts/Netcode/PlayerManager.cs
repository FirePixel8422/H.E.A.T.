using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    public class PlayerManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        private Vector3[] playerSpawnPositions;
        private Quaternion[] playerSpawnRotations;
        private bool spawnPointsActive;


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                (spawnPointsActive, playerSpawnPositions, playerSpawnRotations) = SpawnPointHandler.GetShuffledSpawnPoints();
            }

            SpawnPlayer_ServerRPC(NetworkManager.LocalClientId);
        }


        /// <summary>
        /// Spawn player and destroy all unneeded components on other clients side for your player
        /// </summary>

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        public void SpawnPlayer_ServerRPC(ulong ownerNetworkId)
        {
            if (spawnPointsActive == false) return;

            // 0 for host, 1 for any other
            int arrayId = ownerNetworkId == 0 ? 0 : 1;
            Vector3 pos = playerSpawnPositions[arrayId];
            Quaternion rot = playerSpawnRotations[arrayId];

            NetworkObject spawnedPlayer = NetworkObject.InstantiateAndSpawn(playerPrefab, NetworkManager, ownerNetworkId, position: pos, rotation: rot);
        }
    }
}