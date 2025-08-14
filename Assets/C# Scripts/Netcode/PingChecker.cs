using FirePixel.Networking;
using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Netcode
{
    public class PingChecker : NetworkBehaviour
    {
        private void OnEnable() => UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
        private void OnDisable() => UpdateScheduler.UnregisterFixedUpdate(OnFixedUpdate);


        private void OnFixedUpdate()
        {
            if (IsSpawned == false || MessageHandler.Instance == null || Time.frameCount % 30 != 0) return; 

            ulong ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId);

            MessageHandler.Instance.SendTextLocal("ping: " + ping + "ms");
        }
    }
}