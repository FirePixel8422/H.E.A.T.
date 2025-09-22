using UnityEngine;

public class ClickAndRotate : MonoBehaviour
{

    [SerializeField] private MinMaxFloat clampX;

    public float rotationSpeed = 5f;
    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Update()
    {

        if (Input.GetMouseButton(0))
        {
            // Get mouse movement
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // Update rotation values
            rotationX += mouseY;
            rotationY -= mouseX;
            rotationX = Mathf.Clamp(rotationX, clampX.min, clampX.max);

            // Apply rotation
            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
    }


}
