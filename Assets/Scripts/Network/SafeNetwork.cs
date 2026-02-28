using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

// Manages cooperative safe opening logic and synchronizes progress across players
public class SafeNetwork : NetworkBehaviour
{
    [Header("Settings")]
    public float safeFillTime = 5f;
    public int requiredHolders = 2; // How many players must hold to progress

    [Header("Visual")]
    public GameObject safeParentToDisable; // The visible safe model / door to hide when opened

    // Progress 0..1, server-write only
    public NetworkVariable<float> Progress = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsOpen = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private HashSet<ulong> holders = new HashSet<ulong>();

    private void Update()
    {
        if (!IsServer) return;
        if (IsOpen.Value) return;

        // Progress only if required number of clients hold
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
            // Slowly regress if nobody holding
            Progress.Value = Mathf.Max(0f, Progress.Value - Time.deltaTime * 0.15f);
        }
    }

    private void OnOpenedServer()
    {
        // Notify clients to visually open/disable the safe
        SetSafeOpenClientRpc();

        // Tell GameManager (server) to start the timer
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
