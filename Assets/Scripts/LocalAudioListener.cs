using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(AudioListener))]
public class LocalAudioListener : NetworkBehaviour
{
    AudioListener listener;

    void Awake()
    {
        listener = GetComponent<AudioListener>();
    }

    public override void OnNetworkSpawn()
    {
        // Only enable for the local player
        if (IsOwner)
            listener.enabled = true;
        else
            listener.enabled = false;
    }
}
