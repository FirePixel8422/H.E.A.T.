using System;
using UnityEngine;




public class SpawnPointHandler : MonoBehaviour
{
#pragma warning disable UDR0001
    public static SpawnPointHandler Instance;
#pragma warning restore UDR0001
    private void Awake()
    {
        Instance = this;
    }


    [SerializeField] private Transform[] spawnPoints;


    // Get playerCount Random SpawnsPoints in a random order of current scene
    public static (bool, Vector3[], Quaternion[]) GetShuffledSpawnPoints()
    {
        if (Instance == null || Instance.spawnPoints.Length < 2)
        {
            DebugLogger.LogWarning("No or too little SpawnPoints in this fighting scene found, please place a SpawnPointHandler and assign at least 2 spawnPoints IF you want a network player in this scene");
            return (false, null, null);
        }

        int spawns = Instance.spawnPoints.Length;
        Vector3[] positions = new Vector3[spawns];
        Quaternion[] rotations = new Quaternion[spawns];

        for (int i = 0; i < spawns; i++)
        {
            positions[i] = Instance.spawnPoints[i].position;
            rotations[i] = Instance.spawnPoints[i].rotation;
        }

        // Trim or expand to match player limit
        Array.Resize(ref positions, GlobalGameSettings.MaxPlayers);
        Array.Resize(ref rotations, GlobalGameSettings.MaxPlayers);

        // Optional: shuffle both arrays with the same random order
        for (int i = 0; i < positions.Length - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, positions.Length);
            (positions[i], positions[j]) = (positions[j], positions[i]);
            (rotations[i], rotations[j]) = (rotations[j], rotations[i]);
        }

        return (true, positions, rotations);
    }
}