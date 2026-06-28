using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GiantAttackStrategy : AttackStrategyBase
{
    [Header("Fallback Strategy")]
    [SerializeField] private AttackStrategyBase fallbackMeleeStrategy;
    [SerializeField] private float meleeRange = 2.0f;

    [Header("Charge Attack")]
    [SerializeField] private float chargeMinDistance = 3.5f;
    [SerializeField] private float chargeDamage = 25f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeMaxDuration = 3.0f;
    [SerializeField] private float chargeHitRadius = 1.5f;
    [SerializeField] private float chargeCooldown = 4.0f;

    [Header("Charge Timings")]
    [SerializeField] private float windUpTime = 1.0f;
    [SerializeField] private float impactRecoveryTime = 1.0f;
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject impactVFX;

    private float currentChargeCooldownTimer = 0f;
    private bool isCharging = false;

    public override bool IsAttacking
    {
        get => isCharging || (fallbackMeleeStrategy != null && fallbackMeleeStrategy.IsAttacking);
        protected set => isCharging = value;
    }

    private void Update()
    {
        if (currentChargeCooldownTimer > 0)
        {
            currentChargeCooldownTimer -= Time.deltaTime;
        }
    }

    public override void ExecuteAttack(float damage, Transform target)
    {
        if (IsAttacking || target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget >= chargeMinDistance && currentChargeCooldownTimer <= 0f)
        {
            float calculatedDuration = (distanceToTarget / chargeSpeed) + 0.2f;
            float finalDuration = Mathf.Min(calculatedDuration, chargeMaxDuration);

            StartCoroutine(ChargeRoutine(target, finalDuration));
        }
        else if (distanceToTarget <= meleeRange)
        {
            if (fallbackMeleeStrategy != null)
            {
                agent.isStopped = true;
                fallbackMeleeStrategy.ExecuteAttack(damage, target);
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
    }

    private IEnumerator ChargeRoutine(Transform target, float dashDuration)
    {
        isCharging = true;
        currentChargeCooldownTimer = chargeCooldown;

        agent.isStopped = true;

        animator.SetTrigger("ChargeWindUp");

        float timer = 0;
        while (timer < windUpTime)
        {
            if (target != null)
            {
                Vector3 lookPos = target.position - transform.position;
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 10f);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetTrigger("ChargeDash");
        GetComponent<EnemyAudio>().PlayDash();
        Vector3 dashDirection = transform.forward;
        float dashTimer = 0f;
        bool hasHit = false;

        while (dashTimer < dashDuration)
        {
            agent.Move(dashDirection * chargeSpeed * Time.deltaTime);

            Vector3 checkPos = transform.position + transform.forward * 0.5f + Vector3.up;
            Collider[] hits = Physics.OverlapSphere(checkPos, chargeHitRadius);

            foreach (var hit in hits)
            {
                if (hit.GetComponentInParent<EnemyMarker>() == null && hit.GetComponentInParent<IDamageable>() != null)
                {
                    hasHit = true;
                    hit.GetComponentInParent<IDamageable>().TakeDamage(chargeDamage, transform.position);

                    if (impactVFX != null) Instantiate(impactVFX, hit.transform.position, Quaternion.identity);
                    Debug.Log("<color=red>The giant has hit the player!</color>");
                    break;
                }
            }

            if (hasHit) break;

            dashTimer += Time.deltaTime;
            yield return null;
        }

        animator.SetTrigger("ChargeImpact");

        yield return new WaitForSeconds(impactRecoveryTime);

        isCharging = false;
        agent.isStopped = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chargeMinDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0.5f + Vector3.up, chargeHitRadius);
    }
}