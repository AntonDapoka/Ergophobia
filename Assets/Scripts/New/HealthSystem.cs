using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int healthMax;
    [SerializeField] private int healthCurrent;
    [SerializeField] private bool isInvincible = false;



    public UnityEvent<int, int> OnHealthChanged = new UnityEvent<int, int>(); // Current, Max
    public UnityEvent OnDeath = new UnityEvent();

    public bool IsDead { get; private set; } = false;

    private void Start()
    {
        healthCurrent = healthMax;
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (IsDead || isInvincible) return;

        healthCurrent = Mathf.Max(0, healthCurrent - amount);
        OnHealthChanged.Invoke(healthCurrent, healthMax);

        if (healthCurrent == 0)
        {
            IsDead = true;
            OnDeath.Invoke();

        }
    }

    public void SetInvincibility(bool state)
    {
        isInvincible = state;
    }
}