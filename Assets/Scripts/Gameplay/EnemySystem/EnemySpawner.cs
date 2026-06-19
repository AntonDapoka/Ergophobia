using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyPrefabEntry> enemyPrefabs = new();
    [SerializeField] private LevelTransitionManager transitionManager;
    [SerializeField] private LevelLayoutGenerationScript levelGenerator;
    [SerializeField] private Transform holder;
    [SerializeField] private float offset;

    private Dictionary<RoomScript, bool> spawnedRooms = new();
    private Dictionary<RoomScript, List<GameObject>> spawnedEnemies = new();

    private void OnEnable()
    {
        if (transitionManager != null) transitionManager.OnRoomEntered += HandleRoomEntered;

        if (levelGenerator != null) levelGenerator.OnLevelGenerated += HandleLevelGenerated;
    }

    private void OnDisable()
    {
        if (transitionManager != null) transitionManager.OnRoomEntered -= HandleRoomEntered;

        if (levelGenerator != null) levelGenerator.OnLevelGenerated -= HandleLevelGenerated;
    }

    private void HandleLevelGenerated(List<RoomScript> rooms)
    {
        foreach (var kvp in spawnedEnemies)
            foreach (GameObject enemy in kvp.Value)
                if (enemy != null) Destroy(enemy);

        spawnedEnemies.Clear();
        spawnedRooms.Clear();
    }

    private void HandleRoomEntered(RoomScript room)
    {
        if (room == null) return;
        if (spawnedRooms.TryGetValue(room, out bool spawned) && spawned) return;

        SpawnWave(room);
    }

    private void SpawnWave(RoomScript room)
    {
        spawnedRooms[room] = true;

        List<SpawnerPoint> points = room.GetSpawnerPoints();
        if (points == null || points.Count == 0) return;

        Dictionary<int, List<SpawnerPoint>> waveMap = new();
        foreach (SpawnerPoint point in points)
        {
            if (point == null) continue;

            int wave = point.SpawnWave;
            if (!waveMap.ContainsKey(wave))
                waveMap[wave] = new List<SpawnerPoint>();

            waveMap[wave].Add(point);
        }

        if (waveMap.Count == 0) return;

        List<int> waveKeys = new(waveMap.Keys);
        int selectedWave = waveKeys[Random.Range(0, waveKeys.Count)];

        if (!spawnedEnemies.ContainsKey(room))
            spawnedEnemies[room] = new List<GameObject>();

        foreach (SpawnerPoint point in waveMap[selectedWave])
        {
            if (point == null) continue;
            SpawnAt(room, point);
        }
    }

    private void SpawnAt(RoomScript room, SpawnerPoint point)
    {
        GameObject prefab = GetPrefabFor(point.Type);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab configured for enemy type '{point.Type}' on '{point.name}'.", point);
            return;
        }
        Vector3 positionNew = new(point.transform.position.x, point.transform.position.y + offset, point.transform.position.z);
        GameObject enemy = Instantiate(prefab, positionNew, point.transform.rotation, holder);
        spawnedEnemies[room].Add(enemy);
    }

    private GameObject GetPrefabFor(EnemyType type)
    {
        if (enemyPrefabs == null) return null;

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            EnemyPrefabEntry entry = enemyPrefabs[i];
            if (entry != null && entry.prefab != null && entry.type == type) return entry.prefab;
        }
        return null;
    }
}

[System.Serializable]
public class EnemyPrefabEntry
{
    public EnemyType type;
    public GameObject prefab;
}
