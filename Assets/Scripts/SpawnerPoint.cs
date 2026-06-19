using UnityEngine;

public class SpawnerPoint : MonoBehaviour
{
    [SerializeField] private EnemyType type;
    [SerializeField] private int spawnWave;

    public EnemyType Type => type;
    public int SpawnWave => spawnWave;
}
