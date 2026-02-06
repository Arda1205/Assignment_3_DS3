using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode; // NEW

public class PlayerController : NetworkBehaviour // NEW
{
    public float speed;
    public float mouseSense;

    public CharacterController playerController;
    public Transform cameraTransform;

    public Camera playerCamera; // NEW

    float xRotation = 0f;

    public override void OnNetworkSpawn() // NEW
    {
        if(!IsOwner)
        {
            playerCamera.enabled = false;
        }

        // Hide mouse cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /*
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hide mouse cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }*/

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) // NEW
        {
            return;
        }

        Vector2 moveInput = Keyboard.current != null
             ? new Vector2 (
                 (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ? 1 : 0), // a pressed goes -1 in x direction. d for +1 in x direction
                 (Keyboard.current.sKey.isPressed ? -1 : 0) + (Keyboard.current.wKey.isPressed ? 1 : 0) // s pressed goes -1 in y direction. w for +1 in y direction
                 ) : Vector2.zero;

        Vector3 movePlayer = transform.right * moveInput.x + transform.forward * moveInput.y;
        playerController.Move(movePlayer * speed * Time.deltaTime);

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSense * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSense * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp( xRotation, -80f, 80f );

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0 );
        transform.Rotate(Vector3.up * mouseX);
    }
}
