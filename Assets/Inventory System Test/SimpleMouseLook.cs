using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMouseLook : MonoBehaviour
{
    public float sensitivity = 2f;
    public Transform playerBody;

    private Vector2 lookInput;
    private float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Called by PlayerInput
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        // Pitch
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw
        playerBody.Rotate(Vector3.up * mouseX);
    }
}