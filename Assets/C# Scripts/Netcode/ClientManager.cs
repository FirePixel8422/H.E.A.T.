using System;
using Unity.Netcode;
using UnityEngine;


public class ClientManager : NetworkBehaviour
{
    public static ClientManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }


    #region PlayerIdDataArray var get, set and sync methods

    [SerializeField] private NetworkStruct<PlayerIdDataArray> playerIdDataArray = new NetworkStruct<PlayerIdDataArray>();

    /// <summary>
    /// Get PlayerIdDataArray Copy (changes on copy wont sync back to clientManager and wont cause a networkSync unless sent back with  <see cref="SetPlayerIdDataArray_OnServer"/>")
    /// </summary>
    public static PlayerIdDataArray GetPlayerIdDataArray()
    {
        return Instance.playerIdDataArray.Value;
    }

    /// <summary>
    /// Set Value Of PlayerIdDataArray, Must be called from server (Will trigger networkSync)
    /// </summary>
    public static void SetPlayerIdDataArray_OnServer(PlayerIdDataArray newValue)
    {
#if UNITY_EDITOR
        if (Instance.IsServer == false)
        {
            Debug.LogError("UpdatePlayerIdDataArray_OnServer called on non server Client, this should only be called from the server!");
        }
#endif

        Instance.playerIdDataArray.Value = newValue;
    }


    private void SendPlayerIdDataArrayChange_OnServer()
    {
        ReceivePlayerIdDataArray_ClientRPC(playerIdDataArray.Value, NetcodeUtility.SendToAllButHost());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlayerIdDataArray_ServerRPC(ulong clientNetworkId)
    {
        ReceivePlayerIdDataArray_ClientRPC(playerIdDataArray.Value, NetcodeUtility.SendToClient(clientNetworkId));
    }

    [ClientRpc(RequireOwnership = false)]
    private void ReceivePlayerIdDataArray_ClientRPC(PlayerIdDataArray newValue, ClientRpcParams rpcParams = new ClientRpcParams())
    {
        playerIdDataArray.Value = newValue;
    }

    #endregion


    /// <summary>
    /// Turn GameId into NetworkId
    /// </summary>
    public static ulong GetClientNetworkId(int gameId) => Instance.playerIdDataArray.Value.GetPlayerNetworkId(gameId);

    /// <summary>
    /// Turn NetworkId into GameId
    /// </summary>
    public static int GetClientGameId(ulong networkId) => Instance.playerIdDataArray.Value.GetPlayerGameId(networkId);


    #region OnConnect, OnDisconnect and OnKicked Callbacks

#pragma warning disable UDR0001
    [Tooltip("Invoked after NetworkManager.OnClientConnected, before updating ClientManager gameId logic. \nreturns: \nulong clientId, \nint clientGamId, \nint clientInLobbyCount")]
    public static Action<ulong, int, int> OnClientConnectedCallback;

    [Tooltip("Invoked after NetworkManager.OnClientDisconnected, before updating ClientManager gameId logic. \nreturns: \nulong clientId, \nint clientGamId, \nint clientInLobbyCount")]
    public static Action<ulong, int, int> OnClientDisconnectedCallback;

    [Tooltip("Invoked when a client is kicked from the server, before destroying the ClientManager gameObject. \nreturns: \nnone")]
    public static Action OnKicked;
#pragma warning restore UDR0001

    #endregion


    #region Initialization Callback

#pragma warning disable UDR0001
    [Tooltip("Invoked after this scripts OnNetworkSpawn is fully executed")]
    private static Action OnInitialized;
#pragma warning restore UDR0001
    public static bool Initialized { get; private set; }

    /// <summary>
    /// Subscribe to OnInitialized event, if already initialized instead invoke the action (and dont subscribe to OnInitialized) 
    /// </summary>
    public static void SubscribeToOnInitialized(Action toExecuteAction, bool invokeIfAlreadyInitialized = true)
    {
        //if this script is already initialized
        if (Initialized)
        {
            //invoke the action if "invokeIfAlreadyInitialized" is true
            if (invokeIfAlreadyInitialized)
            {
                toExecuteAction.Invoke();
            }

            return;
        }

#pragma warning disable UDR0004
        OnInitialized += toExecuteAction;
#pragma warning restore UDR0004
    }

    #endregion


    #region Usefull Data and LocalClient Data

    [Tooltip("Local Client gameId, the number equal to the clientCount when this client joined the lobby")]
    public static int LocalClientGameId { get; private set; }


    [Tooltip("Amount of Players in server that have been setup by ClientManager (game/team ID System")]
    public static int PlayerCount => Instance.playerIdDataArray.Value.PlayerCount;

    [Tooltip("Amount of Players in server that have been setup is 1 higher then the highestPlayerId")]
    public static ulong UnAsignedPlayerId => (ulong)Instance.playerIdDataArray.Value.PlayerCount;


    [Tooltip("Local Client userName, value is set after ClientDisplayManager's OnNetworkSpawn")]
    public static string LocalUserName { get; private set; }

    private void CreateLocalUsername()
    {
        LocalUserName = PlayerNameHandler.playerName;
    }

    #endregion


    public override void OnNetworkSpawn()
    {
        playerIdDataArray = new NetworkStruct<PlayerIdDataArray>();

        if (IsServer)
        {
            // On value changed event of playerIdDataArray
            playerIdDataArray.OnValueChanged += (PlayerIdDataArray newValue) =>
            {
                LocalClientGameId = newValue.GetPlayerGameId(NetworkManager.LocalClientId);
                SendPlayerIdDataArrayChange_OnServer();
            };

            // Call manually for the server, since its only triggers for each joining client.
            OnClientConnected_OnServer(0);

            // Setup server only events
            NetworkManager.OnClientConnectedCallback += OnClientConnected_OnServer;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnServer;
        }
        else
        {
            // Catches up late joining clients with newest value
            RequestPlayerIdDataArray_ServerRPC(NetworkManager.LocalClientId);

            // On value changed event of playerIdDataArray
            playerIdDataArray.OnValueChanged += (PlayerIdDataArray newValue) =>
            {
                LocalClientGameId = newValue.GetPlayerGameId(NetworkManager.LocalClientId);
            };
        }

        // Setup server and client event
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected_OnClient;

        DontDestroyOnLoad(gameObject);
        CreateLocalUsername();

        // Invoke OnInitialized event after OnNetworkSpawn is fully executed, also Set initialized to true and clear OnInitialized Action
        OnInitialized?.Invoke();
        OnInitialized = null;
        Initialized = true;
    }


    #region Join and Leave Callbacks

    /// <summary>
    /// When a clients joins the lobby, called on the server only
    /// </summary>
    private void OnClientConnected_OnServer(ulong clientId)
    {
        PlayerIdDataArray updatedDataArray = playerIdDataArray.Value;

        updatedDataArray.AddPlayer(clientId);

        playerIdDataArray.Value = updatedDataArray;

        OnClientConnectedCallback?.Invoke(clientId, playerIdDataArray.Value.GetPlayerGameId(clientId), NetworkManager.ConnectedClients.Count);
    }


    /// <summary>
    /// When a client leaves the lobby, called on the server only
    /// </summary>
    private void OnClientDisconnected_OnServer(ulong clientId)
    {
        // If the diconnecting client is the server dont update data, the server is shut down anyways.
        if (clientId == 0) return;

        PlayerIdDataArray updatedDataArray = GetPlayerIdDataArray();

        updatedDataArray.RemovePlayer(clientId);

        playerIdDataArray.Value = updatedDataArray;

        OnClientDisconnectedCallback?.Invoke(clientId, playerIdDataArray.Value.GetPlayerGameId(clientId), PlayerCount);
    }


    /// <summary>
    /// When a client leaves the lobby, called only on disconnecting client
    /// </summary>
    private void OnClientDisconnected_OnClient(ulong clientId)
    {
        // Call function only on client who disconnected
        if (clientId != NetworkManager.LocalClientId)
        {
            return;
        }

        Destroy(gameObject);

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected_OnServer;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnServer;
        }

        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected_OnClient;

        //when kicked from the server, load this scene
        SceneManager.LoadScene("Setup Network");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion


    #region Kick Client and kill Server Code

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient_ServerRPC(ulong clientNetworkId)
    {
        GetKicked_ClientRPC(NetcodeUtility.SendToClient(clientNetworkId));

        NetworkManager.DisconnectClient(clientNetworkId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient_ServerRPC(int clientGameId)
    {
        ulong clientNetworkId = GetClientNetworkId(clientGameId);

        GetKicked_ClientRPC(NetcodeUtility.SendToClient(clientNetworkId));

        NetworkManager.DisconnectClient(clientNetworkId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectAllClients_ServerRPC()
    {
        GetKicked_ClientRPC();
    }

    [ClientRpc(RequireOwnership = false)]
    private void GetKicked_ClientRPC(ClientRpcParams rpcParams = new ClientRpcParams())
    {
        OnKicked?.Invoke();

        // Destroy the rejoin reference on the kicked client
        FileManager.TryDeleteFile("RejoinData.json");
    }

    #endregion


    public override void OnDestroy()
    {
        base.OnDestroy();

        playerIdDataArray.OnValueChanged = null;
        OnClientConnectedCallback = null;
        OnClientDisconnectedCallback = null;
        OnKicked = null;
        OnInitialized = null;
    }
}