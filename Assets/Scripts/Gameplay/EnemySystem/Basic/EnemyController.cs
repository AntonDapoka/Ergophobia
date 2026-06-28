using NUnit.Framework.Interfaces;
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
    private ShieldComponent shield; 
    private IState currentState;

    private bool isDead = false;

    public Transform CurrentTarget { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<IMovable>();
        PatrolStrategy = GetComponent<IPatrolStrategy>();

        health = GetComponent<HealthComponent>();
        sensor = GetComponent<SensorComponent>();
        shield = GetComponent<ShieldComponent>(); 

        health.OnDeath += HandleDeath;
        sensor.OnPlayerSpotted += HandlePlayerSpotted;
        sensor.OnPlayerLost += HandlePlayerLost;
    }

    private void OnEnable()
    {
        if (shield != null)
        {
            shield.OnStunStart += HandleStunStart;
            shield.OnStunEnd += HandleStunEnd;
        }
    }

    private void OnDisable()
    {
        if (shield != null)
        {
            shield.OnStunStart -= HandleStunStart;
            shield.OnStunEnd -= HandleStunEnd;
        }
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
        CurrentTarget = player; // 【新增】记录目标
        // 如果当前不是晕眩状态，才切换到追击
        if (!(currentState is StunState))
        {
            TransitionToState(new ChaseState(this, player));
        }
    }

    private void HandlePlayerLost()
    {
        CurrentTarget = null; // 【新增】清空目标
        if (!(currentState is StunState))
        {
            TransitionToState(new PatrolState(this));
        }
    }

    // ==========================================
    // 【新增】晕眩事件处理
    // ==========================================
    private void HandleStunStart()
    {
        if (isDead) return;
        TransitionToState(new StunState(this));
    }

    private void HandleStunEnd()
    {
        if (isDead) return;
        if (CurrentTarget != null)
        {
            TransitionToState(new ChaseState(this, CurrentTarget));
        }
        else
        {
            TransitionToState(new PatrolState(this));
        }
    }

    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} has died！");

        GetComponent<EnemyAudio>().PlayDeath();
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
            if (TryGetComponent(out Animator anim)) anim.SetTrigger("Die");
            if (TryGetComponent(out Collider col)) col.enabled = false;
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