using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;

    public Transform cameraTransform; // assign Cinemachine camera

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputValue context)
    {
        moveInput = context.Get<Vector2>();
    }

    private void Update()
    {
        // 🔥 Align player Y rotation with camera
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }

        // Movement relative to camera direction
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}