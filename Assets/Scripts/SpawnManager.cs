using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

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

        // fallback
        return spawnPoints[0].position;
    }
}