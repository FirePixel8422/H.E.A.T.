using FirePixel.Networking;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


[Tooltip("Controls player movement, jumping, crouching, and camera look. Handles Rigidbody physics for movement and gravity. Sends player transforms to the server and synchronizes them with clients.")]
public class PlayerController : NetworkBehaviour
{
    #region Movement

    [Header("Movement Settings")]

    [SerializeField] private float crouchSpeed = 1;
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float sprintSpeed = 4.25f;

    [SerializeField] private float speedChangePower = 75;
    [SerializeField] private float midAirSpeedChangePower = 25;

    /// <summary>
    /// If midair: if sprintjumped: <see cref="sprintSpeed"/> if regular jump: <see cref="moveSpeed"/>, if crouching: <see cref="crouchSpeed"/>, if sprinting: <see cref="sprintSpeed"/>, else: <see cref="moveSpeed"/>.
    /// </summary>
    private float GetTargetMoveSpeed()
    {
        if (IsGrounded == false)
            return sprintJumped ? sprintSpeed : moveSpeed;

        if (crouching)
            return crouchSpeed;

        if (sprinting)
            return sprintSpeed;

        return moveSpeed;
    }

    #endregion


    #region Jump and Gravity

    [Header("Jump and Gravity Settings")]

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float fallGravityMultiplier = 5;

    [SerializeField] private float groundCheckRadius = 0.05f;
    private bool IsGrounded => Physics.CheckSphere(groundCheck.position, groundCheckRadius);

    #endregion


    [Header("Camera and Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject gunHolder;
    [SerializeField] private float maxTiltAngle = 90;
    [SerializeField] private float mouseSensitivity = 1;

    public void SetCameraFOV(float value)
    {
        mainCam.fieldOfView = value;
    }

    private Rigidbody rb;
    private Camera mainCam;
    private NetworkStateMachine stateMachine;

    private Vector2 moveDir;
    private bool IsMoving => moveDir.sqrMagnitude > 0.0001f;

    private bool crouching;
    private bool sprinting;
    private bool sprintJumped;


    #region Input Callbacks and Look and Jump Logic

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveDir = ctx.ReadValue<Vector2>();

        stateMachine.UpdateMovementState(IsMoving, crouching, sprinting);        
    }
    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        crouching = ctx.ReadValueAsButton();

        stateMachine.UpdateMovementState(IsMoving, crouching, sprinting);
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        sprinting = ctx.ReadValueAsButton();

        stateMachine.UpdateMovementState(IsMoving, crouching, sprinting);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && crouching == false && IsGrounded)
        {
            // set sprintJumped to true if player is sprinting, so the player can jump and keep more momentum
            sprintJumped = sprinting;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            stateMachine.Jump();
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

        stateMachine.ShakeGooglyEyes();
    }

    #endregion


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = cameraTransform.GetComponent<Camera>();
        stateMachine = GetComponent<NetworkStateMachine>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner == false) return;

        Cursor.lockState = CursorLockMode.Locked;

        // set gun to correct FOV independent and always in front layer ("Gun")
        int layerId = LayerMask.NameToLayer("Gun");
        gunHolder.layer = layerId;

        foreach (Transform child in gunHolder.transform)
        {
            child.gameObject.layer = layerId;
        }
    }


    private void OnEnable() => UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterFixedUpdate(OnFixedUpdate);

    private void OnFixedUpdate()
    {
        // For player instance on NON owning clients, disable FixedUpdate to save performance
        if (IsOwner == false)
        {
            UpdateScheduler.UnregisterFixedUpdate(OnFixedUpdate);
            return;
        }

        #region Update RigidBody velocity and send Transform Data to ServerRPC

        float rbVelocityY = rb.velocity.y;
        float targetMoveSpeed = GetTargetMoveSpeed();

        // Calculate the target velocity based on the input direction and target speed (crouched, sprint, or normal)
        Vector3 targetMovementVelocity = new Vector3(moveDir.x * targetMoveSpeed, rbVelocityY, moveDir.y * targetMoveSpeed);

        float targetSpeedChangePower = IsGrounded ? speedChangePower : midAirSpeedChangePower;

        // Move Rigidbodys velocity to targetMovementVelocity
        rb.velocity = VectorLogic.InstantMoveTowards(rb.velocity, transform.TransformDirection(targetMovementVelocity), targetSpeedChangePower * Time.fixedDeltaTime);

        // If player is falling
        if (rbVelocityY < 0)
        {
            rb.AddForce(Vector3.down * fallGravityMultiplier, ForceMode.Acceleration);
        }

        // Send transform data to server
        SendPlayerTransforms_ServerRPC(transform.position, cameraTransform.localEulerAngles.x, transform.eulerAngles.y, ClientManager.LocalClientGameId);

        #endregion
    }


    #region Send/Recieve Transform Data

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerTransforms_ServerRPC(Vector3 pos, float pitch, float yaw, int fromClientGameId)
    {
        RecievPlayerTransforms_ClientRPC(pos, pitch, yaw, GameIdRPCTargets.SendToAllButClient(fromClientGameId));
    }

    [ClientRpc(RequireOwnership = false)]
    private void RecievPlayerTransforms_ClientRPC(Vector3 pos, float pitch, float yaw, GameIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget == false) return;

        transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
