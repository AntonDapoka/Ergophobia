using UnityEngine;
using UnityEngine.AI;

public class PatrolSystem : AbstractEnemy
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 2f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    protected override void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (patrolPoints.Length > 0)MoveToPoint(patrolPoints[0].position);
        
    }

    private void Update()
    {
        if (agent == null || !agent.enabled) return;
        if (agent.isStopped) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = waitTimeAtPoint;
            }
            else
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    isWaiting = false;
                    GoToNextPoint();
                }
            }
        }
    }

    private void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;
        currentIndex = (currentIndex + 1) % patrolPoints.Length;
        MoveToPoint(patrolPoints[currentIndex].position);
    }

    private void MoveToPoint(Vector3 pos)
    {
        agent.SetDestination(pos);
        agent.isStopped = false;
    }

}