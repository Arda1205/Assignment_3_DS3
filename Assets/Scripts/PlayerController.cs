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
        if (IsServer)
        {
            Vector3 spawnPos = SpawnManager.Instance.GetSpawnPosition(OwnerClientId);
            SetSpawnClientRpc(spawnPos);
        }

        SetupVisuals();
    }

    void SetupVisuals()
    {
        var canvas = GetComponentInChildren<UnityEngine.Canvas>(true);
        if (canvas != null)
            canvas.gameObject.SetActive(IsOwner);

        var capsuleRenderer = GetComponentInChildren<MeshRenderer>();
        if (capsuleRenderer != null)
            capsuleRenderer.enabled = !IsOwner;

        if (!IsOwner)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            return;
        }

        if (playerCamera != null) playerCamera.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        if (!IsOwner) return;

        if (Time.timeScale == 0f) return;

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

        // normalize so diagonal isn't faster
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

        // vertical
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // horizontal (rotate body in place)
        transform.localRotation *= Quaternion.Euler(0f, mouseX, 0f);
    }

    [ClientRpc]
    void SetSpawnClientRpc(Vector3 spawnPos)
    {
        if (!IsOwner) return;

        playerController.enabled = false;
        transform.position = spawnPos;
        playerController.enabled = true;
    }
}
