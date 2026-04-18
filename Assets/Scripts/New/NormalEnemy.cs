using UnityEngine;

public class NormalEnemy : AbstractEnemy
{
    [Header("Combat Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldownDuration = 1.5f; 
    [SerializeField] private int attackDamage;

    // Local Timer
    [SerializeField] private CooldownTimer attackTimer = new CooldownTimer();

    private enum State {Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    protected override void Awake()
    {
        base.Awake();

        attackTimer.SetCooldown(0f);
    }

    private void Update()
    {
        if (healthComponent.IsDead || playerTransform.position == null) return;

        attackTimer.Tick(Time.deltaTime);

   
        float distanceToTarget = Vector3.Distance(transform.position, playerTransform.position);
        bool isTargetDetected = distanceToTarget <= detectionRange;

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrolState(isTargetDetected);
                break;

            case State.Chase:
                UpdateChaseState(distanceToTarget, isTargetDetected);
                break;

            case State.Attack:
                UpdateAttackState(distanceToTarget, isTargetDetected);
                break;
        }
    }



    private void UpdatePatrolState(bool isTargetDetected)
    {

        if (isTargetDetected)
        {
            SwitchState(State.Chase);
        }
    }

    private void UpdateChaseState(float distanceToTarget, bool isTargetDetected)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(playerTransform.position);


        if (!isTargetDetected)
        {
            SwitchState(State.Patrol);
        }

        else if (distanceToTarget <= attackRange)
        {
            SwitchState(State.Attack);
        }
    }

    private void UpdateAttackState(float distanceToTarget, bool isTargetDetected)
    {
        navMeshAgent.isStopped = true; 
        LookAtTarget();


        if (!isTargetDetected || distanceToTarget > attackRange)
        {
            SwitchState(State.Chase);
        }

        else if (attackTimer.IsReady())
        {
            PerformAttack();

            attackTimer.SetCooldown(attackCooldownDuration);
        }
    }


    private void SwitchState(State newState)
    {
        if (currentState == newState) return;

        if (currentState == State.Patrol)
        {
            if (patrolComponent != null) patrolComponent.enabled = false;
        }

        currentState = newState;


        if (currentState == State.Patrol)
        {
            if (patrolComponent != null) patrolComponent.enabled = true;
        }

        Debug.Log($"Enemy State Changed: {currentState}");
    }

    private void LookAtTarget()
    {
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void PerformAttack()
    {
        Debug.Log("Enemy Attacking");
        if (playerTransform.position != null )
        {
            healthComponent.TakeDamage(attackDamage, playerTransform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<BulletMarker>(out BulletMarker bullet))
        {
            healthComponent.TakeDamage(amount: bullet.damage, collision.contacts[0].point);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}