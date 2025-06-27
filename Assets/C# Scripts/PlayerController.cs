using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private float crouchSpeed = 3;
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float sprintSpeed = 3;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpForce = 5;

    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxTiltAngle = 90;
    [SerializeField] private float mouseSensitivity = 1;

    private bool IsGrounded => Physics.CheckSphere(groundCheck.position, 0.01f);

    private Rigidbody rb;
    private Vector2 moveDir;

    private bool crouching;
    private bool sprinting;


    #region Input Handling

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveDir = ctx.ReadValue<Vector2>();
    }
    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        crouching = ctx.ReadValueAsButton();
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        sprinting = ctx.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && crouching == false && IsGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        Vector2 mouseMovement = ctx.ReadValue<Vector2>();

        Vector3 camEuler = cameraTransform.eulerAngles;
        camEuler.x -= mouseMovement.y * mouseSensitivity;

        // Normalize angle from 0-360 to -180 to 180
        if (camEuler.x > 180f)
            camEuler.x -= 360f;

        camEuler.x = math.clamp(camEuler.x, -maxTiltAngle, maxTiltAngle);

        cameraTransform.eulerAngles = camEuler;

        transform.Rotate(Vector3.up, mouseMovement.x * mouseSensitivity);
    }

    #endregion


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        rb = GetComponent<Rigidbody>();
    }


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);

    private void OnUpdate()
    {
        // If crouching move at crouchSpeed, if not crouching and if sprinting move at sprintSpeed, otherwise move at moveSpeed
        float targetMoveSpeed = crouching ? crouchSpeed : (sprinting ? sprintSpeed : moveSpeed);
        
        rb.velocity = transform.TransformDirection(new Vector3(moveDir.x * targetMoveSpeed, rb.velocity.y, moveDir.y * targetMoveSpeed));
    }
}
