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

#pragma warning disable UDR0001
        public static string playerName;
#pragma warning restore UDR0001


        private async void Start()
        {
            (bool success, ValueWrapper<string> savedPlayerName) = await FileManager.LoadInfo<ValueWrapper<string>>("PlayerName.fpx");

            if (success)
            {
                nameInputField.text = savedPlayerName.Value;
                playerName = savedPlayerName.Value;
            }
            else
            {
                string funnyName = GetRandomFunnyName();

                previewTextField.text = funnyName;
                playerName = funnyName;
            }
        }


        public async void OnInputFieldChanged(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                string funnyName = GetRandomFunnyName();

                previewTextField.text = funnyName;
                playerName = funnyName;

                FileManager.TryDeleteFile("PlayerName.fpx");

                return;
            }

            playerName = newValue;
            await FileManager.SaveInfo(new ValueWrapper<string>(newValue), "PlayerName.fpx");
        }

        private string GetRandomFunnyName()
        {
            int r = EzRandom.Range(0, funnyNames.Length);
            return funnyNames[r];
        }
    }
}