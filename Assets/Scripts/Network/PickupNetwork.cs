using Unity.Netcode;
using UnityEngine;

public class PickupNetwork : NetworkBehaviour
{
    [Header("Pickup")]
    public int valueAmount = 1000;

    private bool picked = false;

    [ServerRpc(RequireOwnership = false)]
    public void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        if (picked) return;
        picked = true;

        ulong clientId = rpcParams.Receive.SenderClientId;

        // Give money
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerObj = client.PlayerObject;
            if (playerObj != null)
            {
                var state = playerObj.GetComponent<PlayerState>();
                if (state != null)
                {
                    state.Money.Value += valueAmount;
                }
            }
        }

        // Instead of Despawn, force disable for everyone
        DisablePickupClientRpc();
    }

    [ClientRpc]
    void DisablePickupClientRpc()
    {
        // disable visuals + collider so it can't be picked again
        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        gameObject.SetActive(false);
    }
}