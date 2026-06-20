using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovementComponent : MonoBehaviour, IMovable
{
   private static readonly int SpeedHash = Animator.StringToHash("Speed");
   private NavMeshAgent agent;
    public Animator animator; 

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (animator != null)
            animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }

    public void MoveTo(Vector3 destination, float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public bool IsAtDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}