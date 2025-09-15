using FirePixel.Networking;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    public GameObject[] mainScreens;

    #region MainScreen
    public void GoToLobbyButton()
    {
        mainScreens[0].SetActive(false);
        mainScreens[1].SetActive(true);
    }
    public void GoToArmory()
    {

    }
    public void GoToSettings()
    {

    }
    public void GoToCredits()
    {

    }
    public void ExitGame()
    {
        Application.Quit();
    }
    #endregion
    #region LobbyScreens
    public void CreateRoom()
    {
        mainScreens[1].SetActive(false);
        mainScreens[2].SetActive(true);

        //LobbyMaker.Instance.CreateLobbyAsync();
    }
    public void GoJoinRoomScreen()
    {

    }
    public void StartGame()
    {

    }
    //ALLE BACKBUTTONS HIERONDER
    public void BackToMain()
    {
        mainScreens[1].SetActive(false);
        mainScreens[0].SetActive(true);
    }
    public void BackToLobbySearchCreate()
    {
        mainScreens[2].SetActive(false);
        mainScreens[1].SetActive(true);
    }
    public void BackToLobbySearchJoin()
    {

    }
    #endregion
}
