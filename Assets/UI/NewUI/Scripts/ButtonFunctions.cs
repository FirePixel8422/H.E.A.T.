using FirePixel.Networking;
using System.Collections;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    public GameObject[] mainScreens;
    public Animator animator;

    public float[] animationWaitTime;

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
    #region Armory
    // als je van weapon select gaat naar attachment vergeet niet de offset van -0.2 naar 0.2 te zetten van de cinemachinecamera

    //ALLE BACKBUTTONS HIERONDER
    public void GoBackFromArmory()
    {
        animator.SetInteger("UI", 2);

        mainScreens[3].SetActive(false);
    }
    #endregion

    IEnumerator WaitForArmoryAnim(float time)
    {
        yield return new WaitForSeconds(time);

        mainScreens[3].SetActive(true);
    }
}
