using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

// Provides spawn positions for players based on their client ID
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSpawnPosition(ulong clientId)
    {
        // Simple 2-player setup
        if (clientId < (ulong)spawnPoints.Length)
            return spawnPoints[clientId].position;

        // Fallback
        return spawnPoints[0].position;
    }
}