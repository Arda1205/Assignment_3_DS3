using UnityEngine;
using Unity.Netcode;

// Handles the logic for when a player reaches the exit
public class PlayerEscape : NetworkBehaviour
{
    // Player has successfully escaped?
    public NetworkVariable<bool> HasEscaped = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Stores how much time was remaining when the player escaped
    public NetworkVariable<float> EscapeTimeLeft = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Called locally when the player touches the exit trigger
    [System.Obsolete]
    public void RequestEscape()
    {
        if (!IsOwner) return;

        RequestEscapeServerRpc();
    }

    // Server validates and processes the escape request
    [ServerRpc(RequireOwnership = false)]
    [System.Obsolete]
    private void RequestEscapeServerRpc(ServerRpcParams rpcParams = default)
    {
        if (HasEscaped.Value) return;

        ulong clientId = rpcParams.Receive.SenderClientId;

        // Escape is only valid if the game timer is currently running
        if (GameManager.Instance != null &&
            GameManager.Instance.TimerRunning.Value)
        {
            HasEscaped.Value = true;

            EscapeTimeLeft.Value = GameManager.Instance.TimerValue.Value;

            // Notify GameManager so it can update UI and check win conditions
            GameManager.Instance.RegisterEscape(clientId);
        }
    }
}