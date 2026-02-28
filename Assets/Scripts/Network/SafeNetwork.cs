using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SafeNetwork : NetworkBehaviour
{
    [Header("Settings")]
    public float safeFillTime = 5f;
    public int requiredHolders = 2; // how many players must hold to progress

    [Header("Visual")]
    public GameObject safeParentToDisable; // the visible safe model / door to hide when opened

    // progress 0..1, server-write only
    public NetworkVariable<float> Progress = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsOpen = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private HashSet<ulong> holders = new HashSet<ulong>();

    private void Update()
    {
        if (!IsServer) return;
        if (IsOpen.Value) return;

        // progress only if required number of clients hold
        if (holders.Count >= requiredHolders)
        {
            Progress.Value = Mathf.Clamp01(Progress.Value + (Time.deltaTime / safeFillTime));
            if (Progress.Value >= 1f)
            {
                IsOpen.Value = true;
                OnOpenedServer();
            }
        }
        else
        {
            // optional: slowly regress if nobody holding
            Progress.Value = Mathf.Max(0f, Progress.Value - Time.deltaTime * 0.15f);
        }
    }

    private void OnOpenedServer()
    {
        // notify clients to visually open/disable the safe
        SetSafeOpenClientRpc();

        // tell GameManager (server) to start the timer
        if (GameManager.Instance != null)
            GameManager.Instance.StartTimerServerRpc();
    }

    [ClientRpc]
    void SetSafeOpenClientRpc()
    {
        if (safeParentToDisable != null)
            safeParentToDisable.SetActive(false);

        // Disable auto hold on the LOCAL player
        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer != null)
        {
            var interaction = localPlayer.GetComponent<PlayerInteraction>();
            if (interaction != null)
                interaction.DisableAutoHold();
        }
    }

    // Called by clients when they start holding E
    [ServerRpc(RequireOwnership = false)]
    public void StartHoldServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        holders.Add(clientId);
    }

    // Called by clients when they stop holding E
    [ServerRpc(RequireOwnership = false)]
    public void StopHoldServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        holders.Remove(clientId);
    }
}
