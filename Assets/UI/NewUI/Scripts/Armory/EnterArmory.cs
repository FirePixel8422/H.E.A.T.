using UnityEditor;
using UnityEngine;

public class EnterArmory : MonoBehaviour
{
    private bool armoryIsOpen;
    public float interactionDistance;
    public LayerMask playerLayer;

    [Header("UI")]
    public ArmoryBehavior armoryBehavior;
    public GameObject eToInteract;
    public GameObject armoryCanvas;

    [Header("Camera")]
    public GameObject[] cameras;

    [Header("Player")]
    public GameObject armoryPlayer;
    void Update()
    {
        if (!armoryIsOpen)
        {
            if (Physics.CheckSphere(transform.position, interactionDistance, playerLayer))
            {
                eToInteract.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    armoryIsOpen = true;

                    armoryCanvas.SetActive(true);
                    eToInteract.SetActive(false);
                    armoryPlayer.SetActive(false);

                    cameras[0].SetActive(false);
                    cameras[1].SetActive(true);

                    Cursor.lockState = CursorLockMode.None;

                    armoryBehavior.SetActiveGun();
                }
            }
            else
            {
                eToInteract.SetActive(false);
            }
        }
    }
    public void GoBackFromArmory()
    {
        armoryIsOpen = false;

        armoryCanvas.SetActive(false);
        armoryPlayer.SetActive(true);

        foreach (GameObject gun in armoryBehavior.previewGuns)
        {
            gun.SetActive(false);
        }
        cameras[1].SetActive(false);
        cameras[0].SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, interactionDistance);
    }
}
