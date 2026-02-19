using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 5f;
    public Camera playerCamera;
    public CrosshairUI crosshairScript;
    public PickupUI pickupUI;

    // layer mask for interactables only
    public LayerMask interactLayer;

    private GameObject currentLookTarget;
    private string currentType = "";
    private bool holding = false;

    void Update()
    {
        HandleRaycast();

        if (currentLookTarget == null)
        {
            if (holding)
            {
                pickupUI.Cancel();
                holding = false;
            }
            return;
        }

        if (Keyboard.current.eKey.isPressed)
        {
            holding = true;
            pickupUI.StartInteraction(currentLookTarget, currentType);
            pickupUI.Fill(Time.deltaTime);
        }
        else
        {
            if (holding)
            {
                pickupUI.Cancel();
                holding = false;
            }
        }
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // ONLY hit objects on Interactable layer
        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            GameObject obj = hit.collider.gameObject;

            if (obj.CompareTag("Interactable"))
            {
                SetTarget(obj, "Safe");
                return;
            }
            else if (obj.CompareTag("Money"))
            {
                SetTarget(obj, "Money");
                return;
            }
            else if (obj.CompareTag("Valuable"))
            {
                SetTarget(obj, "Valuable");
                return;
            }
        }

        ClearTarget();
    }

    void SetTarget(GameObject obj, string type)
    {
        currentLookTarget = obj;
        currentType = type;
        crosshairScript.SetInteract(true);
    }

    void ClearTarget()
    {
        if (currentLookTarget != null && holding)
        {
            pickupUI.Cancel();
            holding = false;
        }

        currentLookTarget = null;
        currentType = "";
        crosshairScript.SetInteract(false);
    }
}
