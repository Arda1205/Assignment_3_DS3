using UnityEngine;

// Detects when a player enters the exit trigger and requests an escape from the server
public class ExitZone : MonoBehaviour
{
    [System.Obsolete]
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // The player object should have PlayerEscape component which will call the server RPC
        var escape = other.GetComponent<PlayerEscape>();
        if (escape != null)
        {
            escape.RequestEscape();
        }
    }
}
