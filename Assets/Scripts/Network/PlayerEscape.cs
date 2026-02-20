using UnityEngine;
using Unity.Netcode;

public class PlayerEscape : NetworkBehaviour
{
    // Called locally by ExitZone when player touches exit
    public void RequestEscape()
    {
        if (!IsOwner) return; // only local player calls
        RequestEscapeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestEscapeServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Validate: game must have begun (timer running) or whatever rule you want
        if (GameManager.Instance != null)
        {
            // If timer is not running, maybe allow immediate exit? adjust logic as desired.
            // We'll allow escape only after the safe is open (GameManager.TimerRunning)
            if (GameManager.Instance.TimerRunning.Value)
            {
                // end game for THIS client only
                GameManager.Instance.EndForClient(clientId, "Player escaped with loot");
            }
            else
            {
                // Not allowed yet - optionally send feedback to that client
            }
        }
    }
}
