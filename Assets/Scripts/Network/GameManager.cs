using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float defaultStartTime = 60f;

    // server writes, everyone reads
    public NetworkVariable<float> TimerValue = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> TimerRunning = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (TimerRunning.Value)
        {
            TimerValue.Value = Mathf.Max(0f, TimerValue.Value - Time.deltaTime);

            if (TimerValue.Value <= 0f)
            {
                TimerRunning.Value = false;
                // time ran out -> end game for everyone
                EndGameForAllClientRpc("Time ran out");
            }
        }
    }

    // Called by server to start the global timer
    [ServerRpc(RequireOwnership = false)]
    public void StartTimerServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        TimerValue.Value = defaultStartTime;
        TimerRunning.Value = true;
    }

    // End the game for everyone (clients will freeze)
    [ClientRpc]
    void EndGameForAllClientRpc(string reason)
    {
        GameEndManager.Instance?.EndGame(reason);
    }

    // End game for a single client (targeted)
    [ClientRpc]
    void EndGameForClientClientRpc(string reason, ClientRpcParams clientRpcParams = default)
    {
        GameEndManager.Instance?.EndGame(reason);
    }

    // Server-side helper: request targeted end for a single client id
    public void EndForClient(ulong clientId, string reason)
    {
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };
        EndGameForClientClientRpc(reason, rpcParams);
    }
}
