using UnityEngine;
using UnityEngine.AI;

public abstract class AbstractEnemy : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] protected HealthSystem healthComponent;
    [SerializeField] protected PatrolSystem patrolComponent;
    [SerializeField] protected NavMeshAgent navMeshAgent;

    [SerializeField] private GameObject player;
    protected Transform playerTransform;

    protected void Start()
    {
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    protected virtual void Awake()
    {

        if (healthComponent == null) healthComponent = GetComponent<HealthSystem>();
        if (patrolComponent == null) patrolComponent = GetComponent<PatrolSystem>();
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();

        if (healthComponent != null)
        {
            healthComponent.OnDeath.AddListener(HandleDeath);
        }
    }



    protected virtual void HandleDeath()
    {
        Debug.Log($"{gameObject.name} has died.");

        Destroy(gameObject);
    }
}
