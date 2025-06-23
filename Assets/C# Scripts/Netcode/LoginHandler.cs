using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoginHandler : MonoBehaviour
{
    private AsyncOperation mainSceneLoadOperation;


    private async void Start()
    {
        mainSceneLoadOperation = SceneManager.LoadSceneAsync("Main Scene", LoadSceneMode.Additive, false);

        mainSceneLoadOperation.completed += (_) =>
        {
            SceneManager.UnLoadSceneAsync("Login Screen");
        };

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignOut();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        mainSceneLoadOperation.allowSceneActivation = true;
    }
}
