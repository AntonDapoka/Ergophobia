using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SummonAttackStrategy : AttackStrategyBase
{
    [Header("Summon Settings")]
    [SerializeField] private GameObject[] minionPrefabs;
    [SerializeField] private int minSummons = 2;        
    [SerializeField] private int maxSummons = 4;        
    [SerializeField] private float summonRadius = 4f;   

    [Header("Timing & Feedback ")]
    [SerializeField] private float castTime = 1.5f;     
    [SerializeField] private float postSummonDelay = 0.5f; 
    [SerializeField] private float summonCooldown = 5f; 
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject summonVFX;      

    public override bool IsAttacking { get; protected set; }

    public override void ExecuteAttack(float damage, Transform target)
    {
        if (!IsAttacking)
        {
            StartCoroutine(SummonRoutine());
        }
    }

    private IEnumerator SummonRoutine()
    {
        IsAttacking = true;

        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(castTime);

        int summonCount = Random.Range(minSummons, maxSummons + 1);

        for (int i = 0; i < summonCount; i++)
        {
            StartCoroutine(SpawnMinion());
        }

        yield return new WaitForSeconds(summonCooldown);

        IsAttacking = false; 
    }

    private IEnumerator SpawnMinion()
    {
        if (minionPrefabs == null || minionPrefabs.Length == 0) yield break;

        GameObject prefabToSpawn = minionPrefabs[Random.Range(0, minionPrefabs.Length)];

        Vector2 randomCircle = Random.insideUnitCircle * summonRadius;
        Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
        {           
            if (summonVFX != null)
            {
                GameObject vfx = Instantiate(summonVFX, hit.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }
            yield return new WaitForSeconds(postSummonDelay);
            Instantiate(prefabToSpawn, hit.position, Quaternion.identity);
        }
    }

    //Debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }
}
