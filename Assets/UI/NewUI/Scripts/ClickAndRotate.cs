using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class ClickAndRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 300f;

    [SerializeField] private float rotX, rotY;
    [SerializeField] private MinMaxFloat clampX, clampY;


    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            print('f'); ;

            // Use raw mouse delta, not Input.GetAxis, for smoother consistent input
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");            

            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
            transform.Rotate(Vector3.right, mouseY * rotationSpeed * Time.deltaTime);

            Vector3 euler = transform.localEulerAngles;
            euler.x = Mathf.Clamp(euler.x, clampX.min, clampX.max);
            euler.y = Mathf.Clamp(euler.y, clampY.min, clampY.max);
            euler.z = 0;
            transform.localEulerAngles = euler;
        }
    }
}
