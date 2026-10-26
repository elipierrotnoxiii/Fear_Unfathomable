using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runningSpeed = 9.0f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.8f;
    [SerializeField] private float gravity = 20.0f; // Ponlo positivo aquí

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float lookAngleLimit = 85.0f;

    private Camera mainCamera;
    private CharacterController characterController;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction runAction;
    private InputAction jumpAction;

    private Vector3 verticalVelocity = Vector3.zero;
    private float cameraPitch = 0.0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = GetComponentInChildren<Camera>();

        // Cache actions from the active Input Actions asset
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        runAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void OnEnable()
    {
        // Subscribe to jump event
        if (jumpAction != null)
            jumpAction.performed += OnJump;

        // Lock and hide cursor for first-person gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        if (jumpAction != null)
            jumpAction.performed -= OnJump;

        // Restore cursor state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        Vector2 lookDelta = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        // Horizontal rotation (Yaw) rotates the entire player body
        transform.Rotate(Vector3.up * (lookDelta.x * lookSensitivity));

        // Vertical rotation (Pitch) rotates only the camera
        cameraPitch -= lookDelta.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -lookAngleLimit, lookAngleLimit);

        if (mainCamera != null)
        {
            mainCamera.transform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        Vector2 moveVector = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        bool isSprinting = runAction != null && runAction.IsPressed();
        float currentSpeed = isSprinting ? runningSpeed : walkSpeed;

        // 1. Desplazamiento horizontal
        Vector3 move = transform.right * moveVector.x + transform.forward * moveVector.y;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }
        characterController.Move(move * (currentSpeed * Time.deltaTime));

        // 2. Mantener pegado al suelo si no está saltando
        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

       
        verticalVelocity.y -= Mathf.Abs(gravity) * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (characterController.isGrounded)
        {
            // Usa Mathf.Abs para garantizar que el valor dentro de Sqrt siempre sea positivo
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * 2f * Mathf.Abs(gravity));
        }
    }
}
