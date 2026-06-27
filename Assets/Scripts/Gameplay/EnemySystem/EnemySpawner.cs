using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyPrefabConfig> enemyPrefabConfigs = new();
    [SerializeField] private LevelTransitionManager transitionManager;
    [SerializeField] private LevelLayoutGenerationScript levelGenerator;
    [SerializeField] private Transform holder;
    [SerializeField] private float offset;

    private Dictionary<RoomScript, bool> spawnedRooms = new();
    private Dictionary<RoomScript, List<GameObject>> spawnedEnemies = new();
    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedPrefabs = new();

    public event Action<RoomScript> OnRoomCleared;
    public event Action<GameObject> OnEnemySpawned;
    public event Action<GameObject> OnEnemyDestroyed;
    public event Action OnAllEnemiesCleared;

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

    private void OnDestroy()
    {
        ReleaseAll();
    }

    public IEnumerator InitializeAsync()
    {
        if (enemyPrefabConfigs == null) yield break;

        foreach (EnemyPrefabConfig config in enemyPrefabConfigs)
        {
            if (config == null || config.prefabReference == null || !config.prefabReference.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"Enemy prefab config '{(config != null ? config.name : "null")}' is invalid.");
                continue;
            }

            string key = config.prefabReference.RuntimeKey.ToString();
            if (!loadedPrefabs.ContainsKey(key))
            {
                AsyncOperationHandle<GameObject> handle = config.prefabReference.LoadAssetAsync<GameObject>();
                loadedPrefabs[key] = handle;
            }
        }

        foreach (AsyncOperationHandle<GameObject> handle in loadedPrefabs.Values)
        {
            if (!handle.IsDone) yield return handle;
        }
    }

    private void HandleLevelGenerated(List<RoomScript> rooms)
    {
        foreach (var kvp in spawnedEnemies)
        {
            foreach (GameObject enemy in kvp.Value)
            {
                if (enemy != null)
                {
                    OnEnemyDestroyed?.Invoke(enemy);
                    Destroy(enemy);
                }
            }
        }

        spawnedEnemies.Clear();
        spawnedRooms.Clear();
        OnAllEnemiesCleared?.Invoke();
    }

    private void HandleRoomEntered(RoomScript room)
    {
        if (room == null) return;
        if (spawnedRooms.TryGetValue(room, out bool spawned) && spawned) return;

        StartCoroutine(SpawnWaveCoroutine(room));
    }

    private IEnumerator SpawnWaveCoroutine(RoomScript room)
    {
        spawnedRooms[room] = true;

        List<SpawnerPoint> points = room.GetSpawnerPoints();
        if (points == null || points.Count == 0) yield break;

        Dictionary<int, List<SpawnerPoint>> waveMap = new();
        foreach (SpawnerPoint point in points)
        {
            if (point == null) continue;

            int wave = point.SpawnWave;
            if (!waveMap.ContainsKey(wave))
                waveMap[wave] = new List<SpawnerPoint>();

            waveMap[wave].Add(point);
        }

        if (waveMap.Count == 0) yield break;

        List<int> waveKeys = new(waveMap.Keys);
        int selectedWave = waveKeys[UnityEngine.Random.Range(0, waveKeys.Count)];

        if (!spawnedEnemies.ContainsKey(room))
            spawnedEnemies[room] = new List<GameObject>();

        foreach (SpawnerPoint point in waveMap[selectedWave])
        {
            if (point == null) continue;
            yield return StartCoroutine(SpawnAtCoroutine(room, point));
        }
    }



    private IEnumerator SpawnAtCoroutine(RoomScript room, SpawnerPoint point)
    {
        EnemyPrefabConfig config = GetConfigFor(point.Type);
        if (config == null || config.prefabReference == null || !config.prefabReference.RuntimeKeyIsValid())
        {
            Debug.LogWarning($"No valid prefab config for enemy type '{point.Type}' on '{point.name}'.", point);
            yield break;
        }

        string key = config.prefabReference.RuntimeKey.ToString();

        if (!loadedPrefabs.TryGetValue(key, out AsyncOperationHandle<GameObject> handle))
        {
            handle = config.prefabReference.LoadAssetAsync<GameObject>();
            loadedPrefabs[key] = handle;
        }

        if (!handle.IsDone)
            yield return handle;

        if (handle.Result == null)
        {
            Debug.LogError($"Failed to load enemy prefab with key '{key}'.", config);
            yield break;
        }

        Vector3 positionNew = new(point.transform.position.x, point.transform.position.y + offset, point.transform.position.z);
        GameObject enemy = Instantiate(handle.Result, positionNew, point.transform.rotation, holder);
        spawnedEnemies[room].Add(enemy);
        OnEnemySpawned?.Invoke(enemy);

        if (enemy.TryGetComponent<HealthComponent>(out var health))
        {
            health.OnDeath += () => HandleEnemyDeath(room, enemy);
        }
    }

    private EnemyPrefabConfig GetConfigFor(EnemyType type)
    {
        if (enemyPrefabConfigs == null) return null;

        for (int i = 0; i < enemyPrefabConfigs.Count; i++)
        {
            EnemyPrefabConfig config = enemyPrefabConfigs[i];
            if (config != null && config.type == type) return config;
        }
        return null;
    }

    private void ReleaseAll()
    {
        foreach (AsyncOperationHandle<GameObject> handle in loadedPrefabs.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        loadedPrefabs.Clear();
    }

    /// <summary>
    /// Returns true if entering this room will spawn enemies that should activate
    /// the block programming system (i.e. the room has spawn points and is not already cleared).
    /// </summary>
    public bool WillSpawnEnemies(RoomScript room)
    {
        if (room == null) return false;

        List<SpawnerPoint> points = room.GetSpawnerPoints();
        if (points == null || points.Count == 0) return false;

        // Already spawned and cleared -> no enemies to fight.
        if (spawnedRooms.TryGetValue(room, out bool spawned) && spawned && !HasLivingEnemies(room))
            return false;

        return true;
    }

    public bool HasLivingEnemies(RoomScript room)
    {
        if (room == null) return false;
        if (!spawnedEnemies.TryGetValue(room, out List<GameObject> enemies)) return false;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            if (enemies[i].activeInHierarchy)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the total number of active enemies across all rooms.
    /// </summary>
    public int GetTotalLivingEnemyCount()
    {
        int count = 0;

        foreach (var kvp in spawnedEnemies)
        {
            foreach (GameObject enemy in kvp.Value)
            {
                if (enemy != null && enemy.activeInHierarchy)
                    count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Returns all currently active enemies across every room.
    /// </summary>
    public IEnumerable<GameObject> GetAllLivingEnemies()
    {
        foreach (var kvp in spawnedEnemies)
        {
            foreach (GameObject enemy in kvp.Value)
            {
                if (enemy != null && enemy.activeInHierarchy)
                    yield return enemy;
            }
        }
    }

    private void HandleEnemyDeath(RoomScript room, GameObject enemy)
    {
        if (room == null) return;

        if (spawnedEnemies.TryGetValue(room, out List<GameObject> enemies))
        {
            enemies.Remove(enemy);
        }

        OnEnemyDestroyed?.Invoke(enemy);

        if (!HasLivingEnemies(room))
        {
            OnRoomCleared?.Invoke(room);
            transitionManager.OnRoomCleared();
        }
    }
}
