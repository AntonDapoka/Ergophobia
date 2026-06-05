using System.Collections;
using UnityEngine;

public class MeleeAttackStrategy : AttackStrategyBase
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform hitboxCenter;

    [Header("Attack Timers")]
    [SerializeField] private float windUpTime = 0.5f;     
    [SerializeField] private float attackDuration = 1.5f; 

    [Header("Hitbox Settings")]
    [SerializeField] private float hitboxRadius = 1.0f;
    [SerializeField] private LayerMask targetLayer;
    [Header("VFX Settings")]
    [SerializeField] private GameObject explosionVFX;

    private float whichAni;
    public override bool IsAttacking { get; protected set; } 

    public override void ExecuteAttack(float damage, Transform target) 
    {
        if (!IsAttacking) StartCoroutine(AttackRoutine(damage));
    }

    public void ExecuteAttack(float damage)
    {
        if (!IsAttacking)
        {
            StartCoroutine(AttackRoutine(damage));
        }
    }

    private IEnumerator AttackRoutine(float damage)
    {
        IsAttacking = true;
        if (animator != null)
        {
            whichAni = Random.Range(1f, 3f);
            animator.SetFloat("AttackType", whichAni);
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(windUpTime);

        Collider[] hits = Physics.OverlapSphere(hitboxCenter.position, hitboxRadius, targetLayer);
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
                Debug.Log($"Has taken a damage of {damage} to {hit.name} £¡</color>");
            }
        }
        float recoveryTime = attackDuration - windUpTime;
        if (recoveryTime > 0)
        {
            yield return new WaitForSeconds(recoveryTime);
        }

        IsAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (hitboxCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitboxCenter.position, hitboxRadius);
        }
    }
}