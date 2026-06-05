using UnityEngine;

public class IdlePatrolStrategy : MonoBehaviour, IPatrolStrategy
{
    public bool HasRoute => false;
    private Vector3 spawnPosition;

    private void Awake()
    {
        spawnPosition = transform.position;
    }

    public Vector3 GetNextWaypoint()
    {
        return spawnPosition;
    }
}