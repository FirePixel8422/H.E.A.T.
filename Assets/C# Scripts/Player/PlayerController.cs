using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private float crouchSpeed = 1;
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float sprintSpeed = 4.25f;

    [SerializeField] private float speedChangePower = 5;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float fallGravityMultiplier = 5;

    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxTiltAngle = 90;
    [SerializeField] private float mouseSensitivity = 1;

    [SerializeField] private float groundCheckRadius = 0.05f;
    private bool IsGrounded => Physics.CheckSphere(groundCheck.position, groundCheckRadius);

    private Rigidbody rb;
    private NetworkStateMachine stateMachine;

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


    private void OnEnable() => UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterFixedUpdate(OnFixedUpdate);

    private void OnFixedUpdate()
    {
        float targetMoveSpeed = GetTargetMoveSpeed();
        float rbVelocityY = rb.velocity.y;

        // Calculate the target velocity based on the input direction and target speed (crouched, sprinting, or normal)
        Vector3 targetMovementVelocity = new Vector3(moveDir.x * targetMoveSpeed, rbVelocityY, moveDir.y * targetMoveSpeed);

        // Move Rigidbodys velocity to targetMovementVelocity
        rb.velocity = VectorLogic.InstantMoveTowards(rb.velocity, transform.TransformDirection(targetMovementVelocity), speedChangePower * Time.fixedDeltaTime);

        //if player is falling
        if (rbVelocityY < 0)
        {
            rb.AddForce(Vector3.down * fallGravityMultiplier, ForceMode.Acceleration);
        }
    }

    private float GetTargetMoveSpeed()
    {
        if (IsGrounded == false)
            return moveSpeed;

        if (crouching)
            return crouchSpeed;

        if (sprinting)
            return sprintSpeed;

        return moveSpeed;
    }
}
