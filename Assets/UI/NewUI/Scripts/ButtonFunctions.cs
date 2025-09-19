using FirePixel.Networking;
using System.Collections;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;
using UnityEngine.UI;
using TMPro;

public class ButtonFunctions : MonoBehaviour
{
    public GameObject[] mainScreens;
    public Animator animator;

    public float[] animationWaitTime;

    [SerializeField] private string enteredRoomCode;
    public TMP_InputField codeInputfield;
    public TMP_Text codeDisplay;

    [Header("Armory")]
    public GameObject[] armoryScreens;

    public GameObject[] previewGuns;
    public GameObject[] previewGunsAttachments;
    int selectedGun;

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
    #region Armory
    public void PreviewGunButton(int GunID)
    {
        foreach(GameObject gun in previewGuns)
        {
            gun.SetActive(false);
        }
        previewGuns[GunID].SetActive(true);
        //previewGuns[GunID]

        selectedGun = GunID;
    }
    public void CustomizeButton()
    {
        previewGunsAttachments[selectedGun].SetActive(true);
        armoryScreens[0].SetActive(false);
        armoryScreens[1].SetActive(true);
    }
    public void AttachmentButton()
    {
        
    }
    //ALLE BACKBUTTONS HIERONDER
    public void GoBackFromArmory()
    {
        animator.SetInteger("UI", 2);

        mainScreens[3].SetActive(false);

        foreach (GameObject gun in previewGuns)
        {
            gun.SetActive(false);
        }
    }
    public void BackToGuns()
    {
        previewGunsAttachments[selectedGun].SetActive(false);
        armoryScreens[1].SetActive(false);
        armoryScreens[0].SetActive(true);
    }
    #endregion

    IEnumerator WaitForArmoryAnim(float time)
    {
        yield return new WaitForSeconds(time);

        mainScreens[3].SetActive(true);

        selectedGun = 0;
        previewGuns[0].SetActive(true);
    }
}
