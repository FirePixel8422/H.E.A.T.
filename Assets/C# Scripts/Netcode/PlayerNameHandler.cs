using TMPro;
using UnityEngine;


namespace FirePixel.Networking
{
    public class PlayerNameHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TextMeshProUGUI previewTextField;

        [SerializeField] private bool nameChosen;

        [SerializeField]
        private string[] funnyNames = new string[]
            {
            "JohnDoe",
            "WillowWilson",
            "BijnaMichael",
            "Yi-Long-Ma",
            "Loading4Ever",
            "DickSniffer",
            "CraniumSnuiver",
            "Moe-Lester",
            "HonkiePlonkie",
            "WhyIsThisHere",
            "TheFrenchLikeBaguette",
            };


        private async void Start()
        {
            (bool success, ValueWrapper<string> savedPlayerName) = await FileManager.LoadInfoAsync<ValueWrapper<string>>("PlayerName.fpx");

            if (success)
            {
                nameInputField.text = savedPlayerName.Value;
                ClientManager.SetLocalUsername(savedPlayerName.Value);
            }
            else
            {
                string funnyName = GetRandomFunnyName();

                previewTextField.text = funnyName;
                ClientManager.SetLocalUsername(funnyName);
            }
        }


        public async void OnInputFieldChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                string funnyName = GetRandomFunnyName();

                previewTextField.text = funnyName;
                ClientManager.SetLocalUsername(funnyName);

                FileManager.TryDeleteFile("PlayerName.fpx");

                return;
            }

            ClientManager.SetLocalUsername(newValue);
            await FileManager.SaveInfoAsync(new ValueWrapper<string>(newValue), "PlayerName.fpx");
        }

        private string GetRandomFunnyName()
        {
            int r = EzRandom.Range(0, funnyNames.Length);
            return funnyNames[r];
        }
    }
}