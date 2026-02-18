using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange;
    public Camera playerCamera;
    public CrosshairUI crosshairScript;

    // Safe interaction
    private SafeOpenRegion currentSafeRegion;
    private bool isHoldingE = false;

    void Update()
    {
        HandleRaycastCrosshair();
        HandleSafeHoldInput();
    }

    // ---------------- RAYCAST CROSSHAIR ----------------
    void HandleRaycastCrosshair()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                crosshairScript.SetInteract(true);
                return;
            }
        }

        crosshairScript.SetInteract(false);
    }

    // ---------------- SAFE HOLD LOGIC ----------------
    void HandleSafeHoldInput()
    {
        if (currentSafeRegion == null)
            return;

        // holding E
        if (Keyboard.current.eKey.isPressed)
        {
            isHoldingE = true;
            currentSafeRegion.FillBar(Time.deltaTime);
        }
        else
        {
            if (isHoldingE)
            {
                currentSafeRegion.StopFilling();
            }

            isHoldingE = false;
        }
    }

    // Called by trigger
    public void SetSafeRegion(SafeOpenRegion region)
    {
        currentSafeRegion = region;
    }

    public void ClearSafeRegion(SafeOpenRegion region)
    {
        if (currentSafeRegion == region)
        {
            currentSafeRegion.StopFilling();
            currentSafeRegion = null;
        }
    }
}
