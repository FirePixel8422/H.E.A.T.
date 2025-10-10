using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class UpgradeUISlot
{
    [SerializeField] private GameObject parentObj;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private Button confirmButton;
    public Button ConfirmButton => confirmButton;


    /// <summary>
    /// Toggle Active state of parent of UI slot
    /// </summary>
    public void SetActive(bool state)
    {
        parentObj.SetActiveSmart(state);
    }


    /// <summary>
    /// Calls <see cref="SetActive(bool)"/> and Updates UI Image and name
    /// </summary>
    public void SetActiveAndUpdateUI(Sprite upgradeSprite, string upgradeName)
    {
        SetActive(true);

        image.sprite = upgradeSprite;
        name.text = upgradeName;
    }
}