using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 5f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    [Header("Combat")]
    public float sensorRadius = 10f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Tooltip("IsRanged")]
    public bool isRanged;
}