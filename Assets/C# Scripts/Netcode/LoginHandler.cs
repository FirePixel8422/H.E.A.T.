using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace FirePixel.Networking
{
    public class LoginHandler : MonoBehaviour
    {
        [SerializeField] private string StartScreenSceneName = "StartScreen";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private AsyncOperation mainSceneLoadOperation;


        private async void Awake()
        {
            mainSceneLoadOperation = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive, false);

            mainSceneLoadOperation.completed += (_) =>
            {
                SceneManager.UnLoadSceneAsync(StartScreenSceneName);
            };

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            mainSceneLoadOperation.allowSceneActivation = true;
        }
    }
}