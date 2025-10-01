using UnityEngine;

public class ArmoryBehavior : MonoBehaviour
{
    public Animator animator;

    [Header("Armory")]
    public GameObject[] armoryScreens;

    public GameObject[] previewGuns;
    public GameObject[] previewGunsAttachments;
    int selectedGun;
    public void SetActiveGun()
    {
        selectedGun = 0;
        previewGuns[0].SetActive(true);
    }
    public void PreviewGunButton(int GunID)
    {
        foreach (GameObject gun in previewGuns)
        {
            gun.SetActive(false);
        }
        previewGuns[GunID].SetActive(true);

        selectedGun = GunID;
    }
    public void CustomizeButton()
    {
        animator.SetInteger("ArmoryUI", 1);

        previewGunsAttachments[selectedGun].SetActive(true);
        armoryScreens[0].SetActive(false);
        armoryScreens[1].SetActive(true);
    }
    public void AttachmentButton()
    {
        armoryScreens[2].SetActive(true);
    }
    public void BackToGuns()
    {
        animator.SetInteger("ArmoryUI", 0);

        previewGunsAttachments[selectedGun].SetActive(false);
        armoryScreens[2].SetActive(false);
        armoryScreens[1].SetActive(false);
        armoryScreens[0].SetActive(true);
    }
}
