using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


namespace FirePixel.Networking
{
    public static class LobbyManager
    {
        public static Lobby CurrentLobby { get; private set; }
        public static string LobbyId => CurrentLobby.Id;



        /// <summary>
        /// Set the lobby reference for host and clients here and start heartbeat coroutine if called from the server (or host)
        /// </summary>
        public static async Task SetLobbyData(Lobby lobby, bool calledFromHost = false)
        {
            CurrentLobby = lobby;

            if (calledFromHost)
            {
#pragma warning disable CS4014
                HeartbeatLobbyTask(CurrentLobby.Id, 25000);
#pragma warning restore CS4014
            }

            await FileManager.SaveInfo(new ValueWrapper<string>(LobbyId), "RejoinData.json");
        }


        /// <summary>
        /// MUST be called on server. Deletes Lobby
        /// </summary>
        public async static Task DeleteLobbyAsync_OnServer()
        {
            await Lobbies.Instance.DeleteLobbyAsync(LobbyId);
        }

        /// <summary>
        /// MUST be called on server. Sets Lobby.IsLocked state
        /// </summary>
        public static async Task SetLobbyLockStateAsync_OnServer(bool isLocked)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsLocked = isLocked,
            };

            try
            {
                // Update the lobby with the new field value
                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating lobby: {e.Message}");
            }
        }


        /// <summary>
        /// Send ping to server every "pingDelayTicks" so it doesnt auto delete itself.
        /// </summary>
        private static async Task HeartbeatLobbyTask(string lobbyId, int pingDelayTicks)
        {
            while (true)
            {
                await Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);

                await Task.Delay(pingDelayTicks);
            }
        }
    }
}