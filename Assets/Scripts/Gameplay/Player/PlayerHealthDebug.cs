using System;
using UnityEngine;

public class PlayerHealthDebug : MonoBehaviour, IDamageable 
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private LoseScript loseScript;
    [SerializeField] private PlayerMoving playerMoving;
    private PlayerAudio playerAudio;
    private Animator anim;
    private PlayerRigController rigController;

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;
    public event Action OnHit;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Start()
    {
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<PlayerAudio>();
        rigController = GetComponent<PlayerRigController>();
        currentHealth = maxHealth;
        OnDeath += loseScript.Lose;
        OnDeath += DisableMovement;
        NotifyHealthChanged();
    }

    private void DisableMovement()
    {
        playerMoving.SetIsAbleToMove(false);
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position); 
    }
    
    public void TakeDamage(float amount, Vector3 damageSourcePosition)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= amount;
        NotifyHealthChanged();

        OnHit?.Invoke();

        if (anim != null) anim.SetTrigger("Hurt");
        if (rigController != null)
        {
            rigController.DisableIKForHit(0.5f);
        }
        if (playerAudio != null) playerAudio.PlayHurt();

        Debug.Log($"Player has been taken a damage {amount} Health remaining {currentHealth} / {maxHealth}");

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        NotifyHealthChanged();
        Debug.Log($"Player healed {amount} Health {currentHealth} / {maxHealth}");
    }

    private void Die()
    {
        Debug.Log("<color=red>Player has died</color>");
        OnDeath?.Invoke();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDestroy()
    {
        OnDeath -= DisableMovement;
        if (loseScript != null)
            OnDeath -= loseScript.Lose;
    }
}
