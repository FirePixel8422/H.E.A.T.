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
    void Update()
    {
        if (!armoryIsOpen)
        {
            if (Physics.CheckSphere(transform.position, interactionDistance, playerLayer))
            {
                Debug.Log("Can Open Armory");
                eToInteract.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    armoryCanvas.SetActive(true);
                    eToInteract.SetActive(false);

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
        armoryCanvas.SetActive(false);
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
