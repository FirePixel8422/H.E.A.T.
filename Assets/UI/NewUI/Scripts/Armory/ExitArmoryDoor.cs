using UnityEngine;

public class ExitArmoryDoor : MonoBehaviour
{
    public Animator animator;
    private bool pressedButton;
    public float interactionDistance;
    public LayerMask playerLayer;

    [Header("UI")]
    public GameObject eToExit;

    [Header("Camera")]
    public GameObject cineMachineCam;
    public GameObject armoryPlayer;
    void Update()
    {
        if (!pressedButton)
        {
            if (Physics.CheckSphere(transform.position, interactionDistance, playerLayer))
            {
                eToExit.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    armoryPlayer.SetActive(false);
                    cineMachineCam.SetActive(true);

                    animator.SetInteger("UI", 2);

                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                eToExit.SetActive(false);
            }
        }
    }
}
