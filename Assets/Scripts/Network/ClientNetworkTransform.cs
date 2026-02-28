using UnityEngine;
using Unity.Netcode.Components;

// Synchronizes player transform data across the network using client authoritative movement
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}