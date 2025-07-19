using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// MB class responsible for creating and joining lobbies
/// </summary>
public class LobbyMaker : MonoBehaviour
{
    public static LobbyMaker Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }


    [SerializeField] private string nextSceneName = "Pre-MainGame";
    [SerializeField] private GameObject invisibleScreenCover;
    [SerializeField] private GameObject rejoinMenu;


    private async void Start()
    {
        (bool fileExists, ValueWrapper<string> lastJoinedLobbyId) = await FileManager.LoadInfo<ValueWrapper<string>>("RejoinData.json");

        if (fileExists)
        {
            // Turn rejoin menu visible and setup rejoin button to call method
            rejoinMenu.SetActive(true);
            rejoinMenu.GetComponentInChildren<Button>().onClick.AddListener(() => RejoinLobbyAsync(lastJoinedLobbyId.Value));
        }
    }

    public async void CreateLobbyAsync()
    {
        invisibleScreenCover.SetActive(true);
        int maxPlayers = GlobalGameSettings.MaxPlayers;

        try
        {
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxPlayers - 1, "europe-west4");
            RelayHostData _hostData = new RelayHostData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            _hostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);


            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = MatchManager.Instance.settings.privateLobby,
                IsLocked = false,

                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "joinCode", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: _hostData.JoinCode)
                    },
                },
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(PlayerNameHandler.playerName + "'s Lobby", maxPlayers, options);

            await LobbyManager.SetLobbyData(lobby, true);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _hostData.IPv4Address,
                _hostData.Port,
                _hostData.AllocationIDBytes,
                _hostData.Key,
                _hostData.ConnectionData);

            NetworkManager.Singleton.StartHost();

            // Load next scene through network, so all joining clients will also load it automatically
            SceneManager.LoadSceneOnNetwork_OnServer(nextSceneName);
        }
        catch (LobbyServiceException e)
        {
            print(e);

            invisibleScreenCover.SetActive(false);
        }
    }

    public async void AutoJoinLobbyAsync()
    {
        invisibleScreenCover.SetActive(true);

        try
        {
            (bool lobbyFound, List<Lobby> lobbies) = await FindLobbiesAsync();

            if (lobbyFound == false)
            {
                CreateLobbyAsync();

                return;
            }

            // Join oldest joinable lobby
            Lobby lobby = lobbies[0];
            await LobbyManager.SetLobbyData(lobby, false);

            string joinCode = lobby.Data["joinCode"].Value;
            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);


            RelayJoinData _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            invisibleScreenCover.SetActive(false);

            print(e);
        }
    }

    private async Task<(bool, List<Lobby>)> FindLobbiesAsync()
    {
        try
        {
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    //Only get open lobbies (non private)
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "-1"),

                    //Only show non locked lobbies (lobbies that are not yet in a started match)
                     new QueryFilter(
                         field: QueryFilter.FieldOptions.IsLocked,
                         op: QueryFilter.OpOptions.EQ,
                         value: "false"),
                },

                Order = new List<QueryOrder>
                {
                    //Show the oldest lobbies first
                    new QueryOrder(true, QueryOrder.FieldOptions.Created),
                    //
                    new QueryOrder(false, QueryOrder.FieldOptions.AvailableSlots),
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

            return (response.Results.Count > 0, response.Results);
        }
        catch (LobbyServiceException e)
        {
            print(e);

            return (false, null);
        }
    }

    public async void JoinLobbyByIdAsync(string lobbyId)
    {
        if (lobbyId.Length != 22) return;

        invisibleScreenCover.SetActive(true);

        try
        {
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);

            string joinCode = lobby.Data["joinCode"].Value;
            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

            RelayJoinData _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            invisibleScreenCover.SetActive(false);

            print(e);
            throw;
        }
    }

    public async void RejoinLobbyAsync(string lobbyId)
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);

            string joinCode = lobby.Data["joinCode"].Value;
            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

            RelayJoinData _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
}