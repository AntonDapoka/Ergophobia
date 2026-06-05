using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform targetPosition;


    private void OnTriggerEnter(Collider other)
    {

        bool isPlayer = other.GetComponentInParent<EnemyMarker>() == null;

        if (!isPlayer)
        {
            return;
        }

        Vector3 spawnPos = targetPosition != null ? targetPosition.position : transform.position;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Debug.Log("<color=green>玩家进入区域，成功生成敌人！</color>");
    }
}