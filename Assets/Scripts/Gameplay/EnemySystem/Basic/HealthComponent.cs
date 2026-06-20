using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHealth;
    public event Action OnDeath;
    public event Action OnHit;

    private ShieldComponent shield;

    [SerializeField] private bool isInvincible = false;

    private void Start()
    {
        shield = GetComponent<ShieldComponent>();
    }

    public void Initialize(float maxHealth)
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void TakeDamage(float amount, Vector3 damageSourcePosition)
    {
        if (isInvincible || currentHealth <= 0) return;

        if (shield != null && shield.TryBlock(damageSourcePosition))
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} be attacked {amount} damage, remaining health: {currentHealth}");

        OnHit?.Invoke();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        currentHealth += amount;
        Debug.Log($"{gameObject.name} healed {amount}, current health: {currentHealth}");
    }
}