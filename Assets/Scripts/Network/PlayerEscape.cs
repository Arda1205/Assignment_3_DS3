using UnityEngine;
using Unity.Netcode;

public class PlayerEscape : NetworkBehaviour
{
    public NetworkVariable<bool> HasEscaped = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<float> EscapeTimeLeft = new NetworkVariable<float>(
    0f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    [System.Obsolete]
    public void RequestEscape()
    {
        if (!IsOwner) return;
        RequestEscapeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    [System.Obsolete]
    private void RequestEscapeServerRpc(ServerRpcParams rpcParams = default)
    {
        if (HasEscaped.Value) return;

        ulong clientId = rpcParams.Receive.SenderClientId;

        if (GameManager.Instance != null &&
            GameManager.Instance.TimerRunning.Value)
        {
            HasEscaped.Value = true;

            EscapeTimeLeft.Value = GameManager.Instance.TimerValue.Value;

            GameManager.Instance.RegisterEscape(clientId);
        }
    }
}