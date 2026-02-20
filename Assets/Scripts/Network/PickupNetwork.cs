using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class PickupNetwork : NetworkBehaviour
{
    [Header("Pickup")]
    public int valueAmount = 1000;
    private bool picked = false;

    // Client calls this to request the server to give them the item
    [ServerRpc(RequireOwnership = false)]
    public void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        if (picked) return;
        picked = true;

        ulong clientId = rpcParams.Receive.SenderClientId;

        // credit money to the player's PlayerState
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

        // if this NetworkObject is spawned by Netcode, do a server-side despawn to notify clients
        if (NetworkObject != null && NetworkManager.Singleton.IsServer && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true); // destroys across clients
        }
        else
        {
            // fallback local destroy
            Destroy(gameObject);
        }
    }

}
