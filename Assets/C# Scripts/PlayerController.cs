using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpForce = 5;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform recoilTransform;
    [SerializeField] private float maxTiltAngle = 90;
    [SerializeField] private float mouseSensitivity = 1;

    private Rigidbody rb;
    private float pitch;
    private Vector2 moveDir;


    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveDir = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
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

            pitch -= mouseDelta.y * mouseSensitivity;
            pitch = math.clamp(pitch, -maxTiltAngle, maxTiltAngle);

            cameraTransform.rotation = Quaternion.Euler(pitch, 0f, 0f);

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
