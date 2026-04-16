using GLTFast.Schema;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    private NavMeshAgent agent;
    private Transform playerTransform; // Player's Transform

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Behavior")]
    public float detectionRadius = 10f;
    public float chaseRange = 15f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public float nextAttackTime = 2f;


    [Header("Health")]
    public int maxHealth = 3;       
    public int currentHealth;       
    public bool isDead = false;     

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

      // Using Script Markers
        PlayerMarker playerMarker = GameObject.FindAnyObjectByType<PlayerMarker>();

        if (playerMarker != null)
        {
              playerTransform = playerMarker.transform;
        }
        else          
        {
            Debug.LogError("Can't Find Objects with PlayerMarker!");
        }

        // Beginning:Patrol
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (playerTransform == null) return;
        // Is player in the range
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool isPlayerInRange = distanceToPlayer <= detectionRadius;

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrolState();
                if (isPlayerInRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                if (isPlayerInRange && distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
                {
                    currentState = EnemyState.Attack;
                }
                else if (!isPlayerInRange)
                {
                    currentState = EnemyState.Patrol;
                }
                break;

            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);

                if (!isPlayerInRange)
                {
                    currentState = EnemyState.Patrol;
                }
                else if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;
        }
    }

    void HandleChaseState(float distanceToPlayer)
    {
        agent.isStopped = false; 
        agent.SetDestination(playerTransform.position);
    }

    void HandleAttackState(float distanceToPlayer)
    {
        agent.isStopped = true; 

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 10f);
        }

        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }


void HandlePatrolState()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }


    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        agent.isStopped = false;
    }

    void PerformAttack()
    {
        Debug.Log("The enemy has launched an attack£¡");
        //Attack
    }

    //Mark the patrol route
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #region Hurt
    void OnCollisionEnter(Collision collision)
    {
          if (isDead) return; 
  
          if (collision.gameObject.TryGetComponent<BulletMarker>(out BulletMarker bullet))
          {
             TakeDamage();
          }
    }

    void TakeDamage()
      {
          currentHealth--;
          Debug.Log($"The enemy is under attack! Current health: {currentHealth}");

         //UpdateUI();
  
         //Play the Hurt animation
 
         if (currentHealth <= 0)
         {
              Die();
          }
     }
 
      void Die()
      {
           isDead = true;   
          agent.isStopped = true;
          // Play the animation
          Destroy(gameObject);


      }
    #endregion
}
