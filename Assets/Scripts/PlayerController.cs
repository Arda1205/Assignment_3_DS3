using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode; // NEW

public class PlayerController : NetworkBehaviour // NEW
{
    [Header("Movement")]
    public float speed = 6f;
    public float mouseSense = 2f;

    public CharacterController playerController;
    public Transform cameraTransform;
    public Camera playerCamera; // NEW

    float xRotation = 0f;

    // --------- Gravity ---------
    [Header("Gravity")]
    public float gravity = -9.81f;
    private float yVelocity;

    public override void OnNetworkSpawn()
    {
        // find player's canvas and enable only for the owner
        var canvas = GetComponentInChildren<UnityEngine.Canvas>(true);
        if (canvas != null)
        {
            canvas.gameObject.SetActive(IsOwner);
        }

        // Local camera only for owner
        if (!IsOwner)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            // we return so we don't lock the cursor for non-local players
            return;
        }

        // For the local owner: lock cursor and enable camera
        if (playerCamera != null) playerCamera.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        if (!IsOwner) return; // NEW

        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        Vector2 moveInput = Keyboard.current != null
            ? new Vector2(
                (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ? 1 : 0),
                (Keyboard.current.sKey.isPressed ? -1 : 0) + (Keyboard.current.wKey.isPressed ? 1 : 0)
              )
            : Vector2.zero;

        // 🔴 normalize so diagonal isn't faster
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // grounded handling
        if (playerController.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f; // keeps player stuck to ground
        }

        // gravity
        yVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = move * speed + Vector3.up * yVelocity;

        playerController.Move(finalMove * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSense;
        float mouseY = mouseDelta.y * mouseSense;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
