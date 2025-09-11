using FirePixel.Networking;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


[Tooltip("Controls player movement, jumping, crouchInput, and camera look. Handles Rigidbody physics for movement and gravity. Sends player transforms to the server and synchronizes them with clients.")]
public class PlayerController : NetworkBehaviour
{
    #region Movement

    [Header("Movement Settings")]

    [SerializeField] private float crouchSpeed = 1;
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float sprintSpeed = 4.25f;

    [SerializeField] private float steerPower = 75;
    [SerializeField] private float midAirSteerPower = 25;

    [Header("What directions is played allowed to sprint at and how fast")]
    [SerializeField] private SprintDirection sprintDirection = SprintDirection.All;
    [SerializeField] private float airSprintMultiplier = 1;

    /// <summary>
    /// Converts user friendly <see cref="sprintDirection"/> enum to a float value for dot product checks.
    /// </summary>
    private float SprintDirectionDotCheck => sprintDirection switch
    {
        SprintDirection.TrueForward => 0.9f,
        SprintDirection.ForwardAndDiagonal => 0.1f,
        SprintDirection.ForwardAndSideways => 0f,
        SprintDirection.AllButBackward => -0.1f,
        SprintDirection.All => -1f,
        _ => -1f
    };

    /// <summary>
    /// If midair: if sprintjumped: <see cref="sprintSpeed"/> if regular jump: <see cref="moveSpeed"/>, if crouchInput: <see cref="crouchSpeed"/>, if sprintInput: <see cref="sprintSpeed"/>, else: <see cref="moveSpeed"/>.
    /// </summary>
    private float GetTargetMoveSpeed()
    {
        if (IsGrounded == false)
            return sprintJumped ? sprintSpeed * airSprintMultiplier : moveSpeed;

        if (crouchInput)
            return crouchSpeed;

        if (sprintInput)
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


    #region Camera Stuff

    [Header("Camera and Look Settings")]
    [SerializeField] private Transform cameraTransform;
    private Camera mainCam;

    [SerializeField] private GameObject gunHolder;
    [SerializeField] private float maxTiltAngle = 90;
    [SerializeField] private float mouseSensitivity = 1;

    public void SetCameraFOV(float value)
    {
        mainCam.fieldOfView = value;
    }

    #endregion


    private Rigidbody rb;
    private NetworkStateMachine stateMachine;

    private Vector2 moveDir;
    private bool IsMoving => moveDir.sqrMagnitude > 0.0001f;
    private bool IsSprinting => sprintInput && IsSprintingAllowed();

    private bool crouchInput;
    private bool sprintInput;

    private bool sprintJumped;


    #region Input Callbacks and Look and Jump Logic

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveDir = ctx.ReadValue<Vector2>();

        stateMachine.UpdateMovementState(IsMoving, crouchInput, IsSprinting);
    }
    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        crouchInput = ctx.ReadValueAsButton();

        stateMachine.UpdateMovementState(IsMoving, crouchInput, IsSprinting, 0.15f);
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        sprintInput = ctx.ReadValueAsButton();

        stateMachine.UpdateMovementState(IsMoving, crouchInput, IsSprinting, 0.15f);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && crouchInput == false && IsGrounded)
        {
            // set sprintJumped to true if player is sprintInput, so the player can jump and keep more momentum
            sprintJumped = sprintInput;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            stateMachine.Jump(0.1f);
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


    #region Initialize (Components and Callbacks)

    private void OnEnable() => ManageUpdateCallbacks(true);
    private void OnDisable() => ManageUpdateCallbacks(false);

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = cameraTransform.GetComponent<Camera>();
        stateMachine = GetComponent<NetworkStateMachine>();
    }

    public override void OnNetworkSpawn()
    {
        ManageUpdateCallbacks(true);

        if (IsOwner == false) return;

        Cursor.lockState = CursorLockMode.Locked;

        // set gun to correct FOV independent and always in front layer ("Gun")
        int layerId = LayerMask.NameToLayer("Gun");
        gunHolder.layer = layerId;

        foreach (Transform child in gunHolder.transform.GetAllChildren())
        {
            child.gameObject.layer = layerId;
        }
    }

    private bool registeredForUpdates = false;
    private void ManageUpdateCallbacks(bool register)
    {
        if (IsSpawned == false) return;

        if (IsOwner)
        {
            if (registeredForUpdates == register) return;

            UpdateScheduler.ManageFixedUpdate(OnFixedUpdate, register);
            registeredForUpdates = register;
        }
        else
        {
            if (registeredForUpdates == register) return;

            UpdateScheduler.ManageUpdate(OnUpdate, register);
            registeredForUpdates = register;
        }
    }

    #endregion


    /// <summary>
    /// FixedUpdate gets executed by Owner of the player obejct. Executes all core logic and sends transform data to the server.
    /// </summary>
    private void OnFixedUpdate()
    {
        // Update RigidBody velocity and send Transform Data to ServerRPC
        
        float rbVelocityY = rb.linearVelocity.y;

        Vector3 targetForwardVelocity = GetForwardDirection();

        // If player is trying to sprint, check if they are allowed to sprint based on "sprintDirection" rules
        if (sprintInput && IsSprintingAllowed(targetForwardVelocity))
        {
            targetForwardVelocity *= moveSpeed;
            targetForwardVelocity.y = rbVelocityY;
        }
        // If player is not allowed to sprint (or not trying to) get targetMoveSpeed based on "GetTargetMoveSpeed"
        else
        {
            targetForwardVelocity *= GetTargetMoveSpeed();
            targetForwardVelocity.y = rbVelocityY;
        }

        float targetSpeedChangePower = IsGrounded ? steerPower : midAirSteerPower;

        rb.linearVelocity = VectorLogic.InstantMoveTowards(rb.linearVelocity, targetForwardVelocity, targetSpeedChangePower * Time.fixedDeltaTime);

        // If player is falling
        if (rbVelocityY < 0)
        {
            rb.AddForce(Vector3.down * fallGravityMultiplier, ForceMode.Acceleration);
        }

        // Send transform data to server
        SendPlayerTransforms_ServerRPC(transform.position, cameraTransform.localEulerAngles.x, transform.eulerAngles.y);
    }


    #region Transformation Utility And Sprinting Checks

    /// <summary>
    /// Calculate the target velocity based on the input direction and target speed (crouched, sprint, or normal), also convert to local space so W is alwys forward (Normalized)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 GetForwardDirection()
    {
        return transform.TransformDirection(new Vector3(moveDir.x, 0, moveDir.y)).normalized;
    }

    /// <summary>
    /// NORMALIZE INPUT, Check if sprintInput is valid based on <see cref="sprintDirection"/> rules using forward move direction"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSprintingAllowed(Vector3 forwardDirection)
    {
        // Calculate what direction we want to move in (Dot product)
        float forwardDot = Vector3.Dot(transform.forward, forwardDirection);

        if (forwardDot < SprintDirectionDotCheck)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Check if sprintInput is valid based on <see cref="sprintDirection"/> rules using forward move direction, use <see cref="GetForwardDirection"/> as forwardDirection input"
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSprintingAllowed()
    {
        return IsSprintingAllowed(GetForwardDirection());
    }

    #endregion


    #region Send/Recieve Transform Data

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerTransforms_ServerRPC(Vector3 pos, float pitch, float yaw)
    {
        RecievePlayerTransforms_ClientRPC(pos, pitch, yaw);
    }

    [ClientRpc(RequireOwnership = false)]
    private void RecievePlayerTransforms_ClientRPC(Vector3 pos, float pitch, float yaw)
    {
        if (IsOwner) return;

        //// Save current state as interpolation starting point
        //interpolationStartPos = transform.position;
        //interpolationStartRot = transform.rotation;
        //interpolationStartCamRot = cameraTransform.localRotation;

        //// Apply target
        //interpolationTargetPos = pos;
        //interpolationTargetRot = Quaternion.Euler(0f, yaw, 0f);
        //interpolationTargetCamRot = Quaternion.Euler(pitch, 0f, 0f);

        //interpolationStartTime = Time.time;
        //interpolationDuration = 0.1f; // match your send rate

        transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    #endregion



    private Vector3 interpolationStartPos;
    private Vector3 interpolationTargetPos;
    private Quaternion interpolationStartRot;
    private Quaternion interpolationTargetRot;
    private Quaternion interpolationStartCamRot;
    private Quaternion interpolationTargetCamRot;
    private float interpolationStartTime;
    private float interpolationDuration = 0.1f;

    private void OnUpdate()
    {
        return;
        float t = (Time.time - interpolationStartTime) / interpolationDuration;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(interpolationStartPos, interpolationTargetPos, t);
        transform.rotation = Quaternion.Slerp(interpolationStartRot, interpolationTargetRot, t);
        cameraTransform.localRotation = Quaternion.Slerp(interpolationStartCamRot, interpolationTargetCamRot, t);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        UpdateScheduler.UnregisterFixedUpdate(OnFixedUpdate);
        UpdateScheduler.UnregisterUpdate(OnUpdate);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

#if UNITY_EDITOR
    [SerializeField] private bool overrideAllowMovementControls;
#endif
}
