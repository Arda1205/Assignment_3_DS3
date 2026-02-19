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

        // add money to the player's PlayerState (server authoritative)
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObj != null)
            {
                var state = playerObj.GetComponent<PlayerState>();
                if (state != null)
                {
                    // server directly updates network variable
                    state.Money.Value += valueAmount;
                }
            }
        }

        // despawn this network object for everyone
        if (NetworkObject != null && NetworkManager.Singleton.IsServer)
        {
            NetworkObject.Despawn(true); // true = destroy on despawn
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
