using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;


namespace FirePixel.Networking
{
    public class LobbyUIMananager : MonoBehaviour
    {
        public static LobbyUIMananager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }


        private LobbyUIPanel[] lobbyUISlots;
        [SerializeField] private int activeLobbyUISlots;


        private void Start()
        {
            lobbyUISlots = GetComponentsInChildren<LobbyUIPanel>(true);
        }

        public void CreateLobbyUI(List<Lobby> lobbies)
        {
            for (int i = 0; i < activeLobbyUISlots; i++)
            {
                lobbyUISlots[i].mainUI.SetActive(false);
            }

            activeLobbyUISlots = lobbies.Count;
            for (int i = 0; i < lobbies.Count; i++)
            {
                lobbyUISlots[i].mainUI.SetActive(true);
                lobbyUISlots[i].lobbyName.text = lobbies[i].Name;
                lobbyUISlots[i].lobbyId = lobbies[i].Id;

                int maxPlayers = lobbies[i].MaxPlayers;
                string creationDate = lobbies[i].Created.ToLocalTime().ToString();
                bool full = lobbies[i].AvailableSlots == 0;

                lobbyUISlots[i].amountOfPlayersInLobby.text = (maxPlayers - lobbies[i].AvailableSlots).ToString() + "/" + maxPlayers.ToString() + (full ? "Full!" : "");
                lobbyUISlots[i].creationDate.text = creationDate;
                lobbyUISlots[i].Full = full;
            }
        }
    }
}


