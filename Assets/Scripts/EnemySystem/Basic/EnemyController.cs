using UnityEngine;

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
        currentState?.Update();
    }

    public void TransitionToState(IState newState)
    {
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
        Debug.Log($"{gameObject.name} has died£¡");
        // Can be extended to "object pool" ?
        Destroy(gameObject);
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
