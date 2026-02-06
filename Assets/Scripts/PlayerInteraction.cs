using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange; // Try 5f?
    public Camera playerCamera;
    public CrosshairUI crosshairScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Shooting an invisible ray that can hit objects. Used to see if the crosshair is pointed at something nearby
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                crosshairScript.SetInteract(true);  // Calling the crosshair scripts bool

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Button button = hit.collider.GetComponent<Button>();

                    button.Press();
                }
                return;
            }
        }

        crosshairScript.SetInteract(false); // Same again but back to normal color
    }
}
