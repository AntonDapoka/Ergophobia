using UnityEngine;

using MG_BlocksEngine2.Environment;

public class TreasureSpawnerScript : MonoBehaviour
{
    [SerializeField] private GameObject prefabTreasure;
    [SerializeField] private Canvas canvasTreasure;
    [SerializeField] private Transform holderLevel;

    [Header("References")]
    [SerializeField] private BlocksReferenceScript blocksReference;
    [SerializeField] private ChestEnvironment chestEnvironment;

    public void SpawnTreasure(SpawnerPoint spawnerPoint)
    {
        GameObject treasureGO = Instantiate(prefabTreasure, spawnerPoint.transform.position, Quaternion.identity, holderLevel);

        if (treasureGO.TryGetComponent<TreasureScript>(out var treasure))
        {
            treasure.Setup(chestEnvironment, blocksReference);
        }
        else
        {
            Debug.LogWarning($"[TreasureSpawnerScript] Spawned treasure prefab '{prefabTreasure.name}' does not have a TreasureScript.", this);
        }
    }
}
