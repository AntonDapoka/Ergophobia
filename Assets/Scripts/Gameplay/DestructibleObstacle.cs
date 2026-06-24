using System;
using UnityEngine;

public class DestructibleObstacle : MonoBehaviour, IDamageable
{
    [Header("Durability")]
    [SerializeField] private int requiredHits = 3;
    private int currentHits;

    [Header("Visual Effects")]
    [SerializeField] private GameObject[] destructionVFXs;

    private bool isBroken = false;

    public event Action OnDeath;

    private void Start()
    {
        currentHits = requiredHits;
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position);
    }

    public void TakeDamage(float amount, Vector3 damageSourcePosition)
    {
        if (isBroken) return;

        currentHits--;

        if (currentHits <= 0)
        {
            BreakObstacle();
        }
    }

    private void BreakObstacle()
    {
        isBroken = true;

        if (destructionVFXs != null && destructionVFXs.Length > 0)
        {
            foreach (var vfxPrefab in destructionVFXs)
            {
                if (vfxPrefab != null)
                {
                    GameObject spawnedVFX = Instantiate(vfxPrefab, transform.position, transform.rotation);
                    Destroy(spawnedVFX, 3f);
                }
            }
        }

        OnDeath?.Invoke();

        Destroy(gameObject);
    }
}