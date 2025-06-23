using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public static class LobbyManager
{
    public static Lobby CurrentLobby { get; private set; }
    public static string LobbyId => CurrentLobby.Id;



    //Set the lobby reference for host and clients here and start heartbeat coroutine if called from the host
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


    public async static Task DeleteLobbyAsync()
    {
        await Lobbies.Instance.DeleteLobbyAsync(LobbyId);
    }

    public static async Task SetLobbyLockStateAsync(bool isLocked)
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



    private static async Task HeartbeatLobbyTask(string lobbyId, int pingDelayTicks)
    {
        while (true)
        {
            await Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            
            await Task.Delay(pingDelayTicks);
        }
    }
}
