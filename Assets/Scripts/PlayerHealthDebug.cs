using System;
using System.Collections;
using UnityEngine;
public class PlayerHealthDebug : MonoBehaviour, IDamageable 
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth ;
    private float currentHealth;


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
        
        Debug.Log($"<color=#00FF00>Player has been taken a damage of {amount} £¡Health remaining: {currentHealth} / {maxHealth}</color>");

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        Debug.Log("<color=red>Player has died£¡</color>");
        OnDeath?.Invoke();
    }
}
