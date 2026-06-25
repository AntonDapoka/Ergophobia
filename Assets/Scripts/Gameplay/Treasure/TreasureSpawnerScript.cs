using UnityEngine;

public class TreasureSpawnerScript : MonoBehaviour
{
    [SerializeField] private GameObject prefabTreasure;
    [SerializeField] private Canvas canvasTreasure;

    [Header("References")]
    [SerializeField] private BlocksReferenceScript blocksReference;

    public void SpawnTreasure(SpawnerPoint spawnerPoint)
    {
        Instantiate(prefabTreasure, spawnerPoint.transform.position, Quaternion.identity);
    }
}
