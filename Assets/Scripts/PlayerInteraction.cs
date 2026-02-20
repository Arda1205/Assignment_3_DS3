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
    private SafeNetwork currentSafeNetwork;
    private string currentType = "";
    private bool holding = false;

    bool debugAutoHold = false;

    void Update()
    {
        HandleRaycast();

        // DEBUG: toggle auto hold with F2
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            debugAutoHold = !debugAutoHold;
            Debug.Log("Auto Hold: " + debugAutoHold);
        }

        if (currentLookTarget == null)
        {
            if (holding)
            {
                // if we were holding on a safe, stop holding server-side
                if (currentSafeNetwork != null)
                    currentSafeNetwork.StopHoldServerRpc();

                pickupUI.Cancel();
                holding = false;
            }
            return;
        }

        bool holdingKey = Keyboard.current.eKey.isPressed || debugAutoHold;

        if (holdingKey)
        {
            if (!holding)
            {
                // starting hold
                holding = true;

                if (currentType == "Safe" && currentSafeNetwork != null)
                {
                    currentSafeNetwork.StartHoldServerRpc();
                }

                pickupUI.StartInteraction(currentLookTarget, currentType);
            }

            // continue filling UI (local visual)
            pickupUI.Fill(Time.deltaTime);
        }
        else
        {
            if (holding)
            {
                // stopping hold
                if (currentType == "Safe" && currentSafeNetwork != null)
                {
                    currentSafeNetwork.StopHoldServerRpc();
                }

                pickupUI.Cancel();
                holding = false;
            }
        }
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            GameObject obj = hit.collider.gameObject;

            // detect safe specially (we need SafeNetwork component)
            SafeNetwork sn = obj.GetComponentInParent<SafeNetwork>();

            if (sn != null && obj.CompareTag("Interactable"))
            {
                SetTarget(obj, "Safe");
                currentSafeNetwork = sn;
                return;
            }
            else if (obj.CompareTag("Money"))
            {
                SetTarget(obj, "Money");
                currentSafeNetwork = null;
                return;
            }
            else if (obj.CompareTag("Valuable"))
            {
                SetTarget(obj, "Valuable");
                currentSafeNetwork = null;
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
            // Stop server hold if it was a safe
            if (currentSafeNetwork != null)
                currentSafeNetwork.StopHoldServerRpc();

            pickupUI.Cancel();
            holding = false;
        }

        currentLookTarget = null;
        currentType = "";
        currentSafeNetwork = null;
        crosshairScript.SetInteract(false);
    }
}
