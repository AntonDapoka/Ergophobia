using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public enum CombatRole { Melee, Ranged }

    [Header("Token Limits")]
    public int maxMeleeAttackers = 1;
    public int maxRangedAttackers = 2;

    [Header("References")]
    private Transform playerTransform;

    private HashSet<GameObject> currentMeleeAttackers = new HashSet<GameObject>();
    private HashSet<GameObject> currentRangedAttackers = new HashSet<GameObject>();

    private List<GameObject> waitingMeleeEnemies = new List<GameObject>();
    private List<GameObject> waitingRangedEnemies = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 自动寻找玩家
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        CleanUpDeadAttackers();
        AssignTokens(); 
    }

    private void CleanUpDeadAttackers()
    {
        currentMeleeAttackers.RemoveWhere(e => e == null);
        currentRangedAttackers.RemoveWhere(e => e == null);
        waitingMeleeEnemies.RemoveAll(e => e == null);
        waitingRangedEnemies.RemoveAll(e => e == null);
    }

    private void AssignTokens()
    {
        // ==========================================
        // 1. 独立分配近战令牌 (保持随机摇号)
        // ==========================================
        while (currentMeleeAttackers.Count < maxMeleeAttackers && waitingMeleeEnemies.Count > 0)
        {
            int randomIndex = Random.Range(0, waitingMeleeEnemies.Count);
            GameObject luckyEnemy = waitingMeleeEnemies[randomIndex];

            waitingMeleeEnemies.RemoveAt(randomIndex);
            currentMeleeAttackers.Add(luckyEnemy);
        }

        // ==========================================
        // 2. 独立分配远程令牌 (优先距离最近)
        // ==========================================
        if (currentRangedAttackers.Count < maxRangedAttackers && waitingRangedEnemies.Count > 0)
        {
            // 如果成功获取到玩家位置，则对排队的远程敌人按距离进行升序排序
            if (playerTransform != null)
            {
                waitingRangedEnemies.Sort((a, b) =>
                {
                    float distA = Vector3.Distance(a.transform.position, playerTransform.position);
                    float distB = Vector3.Distance(b.transform.position, playerTransform.position);
                    return distA.CompareTo(distB); 
                });
            }

            // 依次发放令牌给排在前面的敌人（即最近的敌人）
            while (currentRangedAttackers.Count < maxRangedAttackers && waitingRangedEnemies.Count > 0)
            {
                GameObject closestEnemy = waitingRangedEnemies[0];
                waitingRangedEnemies.RemoveAt(0);
                currentRangedAttackers.Add(closestEnemy);
            }
        }
    }

    public void RegisterForToken(GameObject enemy, CombatRole role)
    {
        if (role == CombatRole.Melee)
        {
            if (!waitingMeleeEnemies.Contains(enemy) && !currentMeleeAttackers.Contains(enemy))
                waitingMeleeEnemies.Add(enemy);
        }
        else
        {
            if (!waitingRangedEnemies.Contains(enemy) && !currentRangedAttackers.Contains(enemy))
                waitingRangedEnemies.Add(enemy);
        }
    }

    public void UnregisterFromToken(GameObject enemy, CombatRole role)
    {
        if (role == CombatRole.Melee) waitingMeleeEnemies.Remove(enemy);
        else waitingRangedEnemies.Remove(enemy);
    }

    public bool HasToken(GameObject enemy, CombatRole role)
    {
        if (role == CombatRole.Melee) return currentMeleeAttackers.Contains(enemy);
        return currentRangedAttackers.Contains(enemy);
    }

    public void ReleaseToken(GameObject enemy, CombatRole role)
    {
        if (enemy == null) return;

        if (role == CombatRole.Melee) currentMeleeAttackers.Remove(enemy);
        else currentRangedAttackers.Remove(enemy);
    }

    public void RemoveEnemyCompletely(GameObject enemy, CombatRole role)
    {
        if (enemy == null) return;

        if (role == CombatRole.Melee)
        {
            currentMeleeAttackers.Remove(enemy);
            waitingMeleeEnemies.Remove(enemy);
        }
        else
        {
            currentRangedAttackers.Remove(enemy);
            waitingRangedEnemies.Remove(enemy);
        }
    }
}