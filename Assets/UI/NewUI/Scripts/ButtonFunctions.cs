using FirePixel.Networking;
using System.Collections;
using UnityEngine;
using TMPro;

public class ButtonFunctions : MonoBehaviour
{
    public GameObject[] mainScreens;
    public Animator animator;

    public float[] animationWaitTime;

    [SerializeField] private string enteredRoomCode;
    public TMP_InputField codeInputfield;
    public TMP_Text codeDisplay;

    [Header("Cameras")]
    public GameObject[] cameras;

    #region MainScreen
    public void GoToLobbyButton()
    {
        mainScreens[0].SetActive(false);
        mainScreens[1].SetActive(true);
    }
    public void GoToArmory()
    {
        animator.SetInteger("UI", 1);

        StartCoroutine(WaitForArmoryAnim(animationWaitTime[0]));
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
        //LobbyMaker.Instance.CreateLobbyAsync();
        //codeDisplay.text = LobbyManager.LobbyCode;

        mainScreens[1].SetActive(false);
        mainScreens[2].SetActive(true);
    }
    public void GoJoinRoomScreen()
    {
        mainScreens[1].SetActive(false);
        mainScreens[4].SetActive(true);
    }
    public async void JoinRoomByCode()
    {
        bool succes = await LobbyMaker.Instance.JoinLobbyByIdAsync(codeInputfield.text);

        if (succes)
        {
            mainScreens[4].SetActive(false);
            mainScreens[2].SetActive(true);
        }
    }
    public async void QuickJoinRoom()
    {
        bool succes = await LobbyMaker.Instance.AutoJoinLobbyAsync();

        if (succes)
        {
            mainScreens[4].SetActive(false);
            mainScreens[2].SetActive(true);
        }
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
        mainScreens[4].SetActive(false);
        mainScreens[1].SetActive(true);
    }
    #endregion

    IEnumerator WaitForArmoryAnim(float time)
    {
        yield return new WaitForSeconds(time);

        cameras[0].SetActive(false);
        cameras[1].SetActive(true);
    }
}
