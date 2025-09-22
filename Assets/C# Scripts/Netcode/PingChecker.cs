using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;


namespace FirePixel.Networking
{
    public class PingChecker : NetworkBehaviour
    {
        [SerializeField] private float updateInterval = 0.5f; // How often to update the ping display
        private float elapsedTime = 0f;

        private TextMeshProUGUI pingTextObj;
        private UnityTransport transport;
        private ulong serverClientId;


        public override void OnNetworkSpawn()
        {
            pingTextObj = GetComponentInChildren<TextMeshProUGUI>();
            transport = NetworkManager.NetworkConfig.NetworkTransport as UnityTransport;
            serverClientId = NetworkManager.NetworkConfig.NetworkTransport.ServerClientId;
        }

        private void OnEnable() => UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
        private void OnDisable() => UpdateScheduler.UnRegisterFixedUpdate(OnFixedUpdate);

        private void OnFixedUpdate()
        {
            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime < updateInterval || IsSpawned == false || MessageHandler.Instance == null) return;

            elapsedTime = 0;

            ulong pingMs = transport.GetCurrentRtt(serverClientId);

            pingTextObj.text = pingMs + "ms";
        }
    }
}