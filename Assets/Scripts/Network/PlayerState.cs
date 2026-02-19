using UnityEngine;
using Unity.Netcode;

public class PlayerState : NetworkBehaviour
{
    public NetworkVariable<int> Money = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Optionally add hooks or helper methods here
}
