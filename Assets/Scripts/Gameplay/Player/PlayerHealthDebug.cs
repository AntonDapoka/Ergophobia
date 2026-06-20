using System;
using UnityEngine;
public class PlayerHealthDebug : MonoBehaviour, IDamageable 
{
    [SerializeField] private float maxHealth ;
    [SerializeField] private float currentHealth;

    public event Action OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position); 
    }
    public void TakeDamage(float amount,Vector3 damageSourcePosition)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= amount;
        
        Debug.Log($"Player has been taken a damage {amount} Health remaining {currentHealth} / {maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        Debug.Log($"Player healed {amount}Health {currentHealth} / {maxHealth}");
    }

    private void Die()
    {
        Debug.Log("<color=red>Player has died</color>");
        OnDeath?.Invoke();
    }
}
