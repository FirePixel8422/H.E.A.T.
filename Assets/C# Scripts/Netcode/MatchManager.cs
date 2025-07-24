using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }


        [Tooltip("Retrieve MatchData")]
        public MatchSettings settings;

        [Tooltip("Default Match Settings, used when no saved settings are found")]
        [SerializeField] private MatchSettings defaultMatchSettings;

        [Header("Where is UI Parent for all UI that holds components for settings")]
        [SerializeField] private RectTransform UITransform;

        private const string SaveDataPath = "SaveData/CreateLobbySettings.fpx";


        private async void Start()
        {
            //load saved MatchSettings, or load default if that doesnt exist.
            settings = await LoadSettingsFromFileAsync();

            UIComponentGroup[] UIInputHandlers = UITransform.GetComponentsInChildren<UIComponentGroup>(true);
            int UIhandlerCount = UIInputHandlers.Length;

            for (int i = 0; i < UIhandlerCount; i++)
            {
                int dataIndex = i;
                UIInputHandlers[i].Init(settings.GetSavedInt(dataIndex));

                UIInputHandlers[i].OnValueChanged += (value) => UpdateMatchSettingsData(dataIndex, value);
            }
        }

        private void UpdateMatchSettingsData(int sliderId, int value)
        {
            settings.SetIntData(sliderId, value);
        }


        /// <summary>
        /// Sync _matchSettings to server
        /// </summary>
        public async override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                await SaveSettingsAsync(settings);
            }
            else
            {
                RequestSyncMatchSettings_ServerRPC(NetworkManager.LocalClientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSyncMatchSettings_ServerRPC(ulong clientNetworkId)
        {
            SyncMatchSettings_ClientRPC(settings, NetworkIdRPCTargets.SendToTargetClient(clientNetworkId));
        }

        [ClientRpc(RequireOwnership = false)]
        private void SyncMatchSettings_ClientRPC(MatchSettings _settings, NetworkIdRPCTargets rpcTargets)
        {
            if (rpcTargets.IsTarget == false) return;

            settings = _settings;
        }



        private async Task<MatchSettings> LoadSettingsFromFileAsync()
        {
            (bool succes, MatchSettings loadedMatchSettings) = await FileManager.LoadInfo<MatchSettings>(SaveDataPath);

            return succes ? loadedMatchSettings : defaultMatchSettings;
        }

        /// <summary>
        /// Settings are saved when creating the lobby
        /// </summary>
        private async Task SaveSettingsAsync(MatchSettings data)
        {
            await FileManager.SaveInfo(data, SaveDataPath);
        }
    }
}