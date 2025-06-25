using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpForce = 5;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxTiltAngle = 90;
    [SerializeField] private float mouseSensitivity = 1;

    private Rigidbody rb;
    private Vector2 moveDir;


    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveDir = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        ///Use overlapSphereNonAlloc or smt else for FPS







        if (ctx.performed && Physics.OverlapSphere(groundCheck.position, 0.01f).Length > 1)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 mouseDelta = ctx.ReadValue<Vector2>();

            Vector3 camEuler = cameraTransform.eulerAngles;
            camEuler.x -= mouseDelta.y * mouseSensitivity;

            // Normalize angle from 0-360 to -180 to 180
            if (camEuler.x > 180f)
                camEuler.x -= 360f;

            camEuler.x = math.clamp(camEuler.x, -maxTiltAngle, maxTiltAngle);

            cameraTransform.eulerAngles = camEuler;

            transform.Rotate(Vector3.up, mouseDelta.x * mouseSensitivity);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        rb = GetComponent<Rigidbody>();
    }


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);

    private void OnUpdate()
    {
        rb.velocity = transform.TransformDirection(new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.y * moveSpeed));
    }
}
