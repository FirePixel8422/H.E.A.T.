using UnityEngine;
using UnityEngine.UI;



[System.Serializable]
public class HotBarSlot
{
    public Image image;


    /// <summary>
    /// Set Color for image. gets applied at next <see cref="UpdateImage(Color, float)"/> call
    /// </summary>
    public void SetImageColor(Color color)
    {
        image.color = color;
    }

    /// <summary>
    /// Update image fill percentage (for cooldown display)
    /// </summary>
    public void UpdateImageFill(float percent01)
    {
        image.fillAmount = percent01;
    }
}