using System.Collections;
using UnityEngine;

public class ExitArmoryDoor : MonoBehaviour
{
    public Animator animator;
    private bool pressedButton;
    public float interactionDistance;
    public LayerMask playerLayer;
    public float waitForAnimTime;

    [Header("UI")]
    public GameObject eToExit;

    [Header("Camera")]
    public GameObject mainCamera;
    public GameObject exitCinemachineCam;
    public GameObject cinemachineCam;
    public GameObject armoryPlayer;
    private void Update()
    {
        if (!pressedButton)
        {
            if (Physics.CheckSphere(transform.position, interactionDistance, playerLayer))
            {
                eToExit.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    armoryPlayer.SetActive(false);
                    cinemachineCam.SetActive(false);
                    mainCamera.SetActive(true);
                    exitCinemachineCam.SetActive(true);
                    StartCoroutine(WaitForAnim(waitForAnimTime));

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
    IEnumerator WaitForAnim(float time)
    {
        yield return new WaitForSeconds(time);
        cinemachineCam.SetActive(true);
        exitCinemachineCam.SetActive(false);
    }
}
