using UnityEngine;

using MG_BlocksEngine2.Environment;

public class TreasureSpawnerScript : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject prefabTreasure;
    [SerializeField] private ChestEnvironment chestEnvironmentTemplate;

    [Header("Scene References")]
    [SerializeField] private Transform holderLevel;
    [SerializeField] private Transform chestSpawnParent;
    [SerializeField] private BlocksReferenceScript blocksReference;

    public static TreasureSpawnerScript Instance { get; private set; }

    public ChestEnvironment ChestEnvironmentTemplate => chestEnvironmentTemplate;
    public Transform ChestSpawnParent => chestSpawnParent;
    public BlocksReferenceScript BlocksReference => blocksReference;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[TreasureSpawnerScript] Duplicate instance on '{name}' destroyed.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (chestEnvironmentTemplate != null)
        {
            if (chestSpawnParent == null)
                chestSpawnParent = chestEnvironmentTemplate.transform.parent;

            chestEnvironmentTemplate.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Spawns a treasure at the given spawner point. Each treasure creates its own
    /// private ChestEnvironment instance when opened.
    /// </summary>
    public void SpawnTreasure(SpawnerPoint spawnerPoint)
    {
        if (prefabTreasure == null)
        {
            Debug.LogWarning("[TreasureSpawnerScript] Treasure prefab is missing.", this);
            return;
        }

        if (chestEnvironmentTemplate == null)
        {
            Debug.LogWarning("[TreasureSpawnerScript] ChestEnvironment template is missing.", this);
            return;
        }

        GameObject treasureGO = Instantiate(prefabTreasure, spawnerPoint.transform.position, Quaternion.identity, holderLevel);
        if (treasureGO.TryGetComponent(out TreasureScript treasure))
            treasure.Setup(chestEnvironmentTemplate, chestSpawnParent, blocksReference);
    }
}
