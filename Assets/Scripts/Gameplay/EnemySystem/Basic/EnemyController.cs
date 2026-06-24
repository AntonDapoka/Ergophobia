using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthComponent), typeof(MovementComponent), typeof(SensorComponent))]
public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyStatsSO Stats;

    public IMovable Movement { get; private set; }
    public IPatrolStrategy PatrolStrategy { get; private set; }
    public AttackStrategyBase AttackStrategy;

    private HealthComponent health;
    private SensorComponent sensor;
    private IState currentState;

    private bool isDead = false;

    private void Awake()
    {
        Movement = GetComponent<IMovable>();
        PatrolStrategy = GetComponent<IPatrolStrategy>();

        health = GetComponent<HealthComponent>();
        sensor = GetComponent<SensorComponent>();

        health.OnDeath += HandleDeath;
        sensor.OnPlayerSpotted += HandlePlayerSpotted;
        sensor.OnPlayerLost += HandlePlayerLost;
    }

    private void Start()
    {
        health.Initialize(Stats.maxHealth);
        sensor.Initialize(Stats.sensorRadius);

        TransitionToState(new PatrolState(this));
    }

    private void Update()
    {
        if (isDead) return;

        currentState?.Update();
    }

    public void TransitionToState(IState newState)
    {
        if (isDead) return;

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    private void HandlePlayerSpotted(Transform player)
    {
        TransitionToState(new ChaseState(this, player));
    }

    private void HandlePlayerLost()
    {
        TransitionToState(new PatrolState(this));
    }

    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} has died£¡");


        if (CombatManager.Instance != null)
        {
            CombatManager.CombatRole role = Stats.isRanged ? CombatManager.CombatRole.Ranged : CombatManager.CombatRole.Melee;
            CombatManager.Instance.RemoveEnemyCompletely(gameObject, role);
        }

        currentState?.Exit();
        currentState = null;

        if (AttackStrategy != null)
        {
            AttackStrategy.StopAllCoroutines();
            AttackStrategy.enabled = false;
        }

        if (TryGetComponent(out NavMeshAgent agent))
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (TryGetComponent(out RagdollComponent ragdoll))
        {
            ragdoll.EnableRagdoll();

        }
        else
        {
            if (TryGetComponent(out Animator anim))
            {
                anim.SetTrigger("Die");
            }

            if (TryGetComponent(out Collider col))
            {
                col.enabled = false;
            }
        }
        Destroy(gameObject, 1.8f);
    }

    private void OnDestroy()
    {
        if (health != null) health.OnDeath -= HandleDeath;
        if (sensor != null)
        {
            sensor.OnPlayerSpotted -= HandlePlayerSpotted;
            sensor.OnPlayerLost -= HandlePlayerLost;
        }
    }
}