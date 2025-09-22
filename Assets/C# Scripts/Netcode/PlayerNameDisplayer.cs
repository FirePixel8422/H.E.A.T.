using UnityEngine;
using Unity.Netcode;
using TMPro;


namespace FirePixel.Networking
{
    public class PlayerNameDisplayer : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI player1, player2;



        public override void OnNetworkSpawn()
        {
            ulong playerNetworkId = NetworkManager.LocalClientId;
            string userName = ClientManager.LocalUserName;

            UpdateUserName(playerNetworkId, userName);
            SendPlayerName_ServerRPC(playerNetworkId, userName);
        }


        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SendPlayerName_ServerRPC(ulong playerNetworkId, string userName)
        {
            SendPlayerName_ClientRPC(playerNetworkId, userName);
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SendPlayerName_ClientRPC(ulong playerNetworkId, string userName)
        {
            // Only send to other player
            if (NetworkManager.LocalClientId == playerNetworkId) return;

            UpdateUserName(playerNetworkId, userName);
        }

        private void UpdateUserName(ulong playerNetworkId, string name)
        {
            if (playerNetworkId == 0)
            {
                player1.text = name;
            }
            else
            {
                player2.text = name;
            }
        }
    }
}