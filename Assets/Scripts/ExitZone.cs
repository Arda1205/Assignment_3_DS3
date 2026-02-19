using UnityEngine;

public class ExitZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Player escaped, end game
        if (GameEndManager.Instance != null)
            GameEndManager.Instance.EndGame("Player escaped with loot");
    }
}
