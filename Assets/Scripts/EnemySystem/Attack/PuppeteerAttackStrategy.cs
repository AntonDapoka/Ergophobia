using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PuppeteerAttackStrategy : AttackStrategyBase
{
    [Header("Puppet Settings")]
    [SerializeField] private GameObject puppetPrefab;      
    [SerializeField] private float spawnRadius;        
    [SerializeField] private GameObject spawnVFX;           

    [Header("Animation & Timing")]
    [SerializeField] private Animator animator;
    [SerializeField] private float castTime;
    [SerializeField] private float postSummonDelay = 0.5f;
    [Header("Visual String ")]
    [SerializeField] private LineRenderer puppetString;     // Use LineRenderer to draw the string from the puppeteer to the puppet
    [SerializeField] private Transform stringStartPoint;    

    private GameObject currentPuppet;    
    private Transform puppetTransform;

    public override bool IsAttacking { get; protected set; }

    private void Update()
    {
        if (puppetString != null && currentPuppet != null && stringStartPoint != null)
        {
            puppetString.enabled = true;
            puppetString.SetPosition(0, stringStartPoint.position);
            puppetString.SetPosition(1, puppetTransform.position + Vector3.up * 1.0f);
        }
        else if (puppetString != null)
        {
            puppetString.enabled = false; 
        }
    }

    public override void ExecuteAttack(float damage, Transform target)
    {
        if (!IsAttacking && currentPuppet == null)
        {
            StartCoroutine(SummonPuppetRoutine());
        }
    }

    private IEnumerator SummonPuppetRoutine()
    {
        IsAttacking = true;
        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(castTime);

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
        {
            if (spawnVFX != null)
            {
                GameObject vfx = Instantiate(spawnVFX, hit.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }
            yield return new WaitForSeconds(postSummonDelay);

            currentPuppet = Instantiate(puppetPrefab, hit.position, Quaternion.identity);
            puppetTransform = currentPuppet.transform;      
        }

        IsAttacking = false;
    }
    private void OnDestroy()
    {
        if (currentPuppet != null)
        { 

            Destroy(currentPuppet);
        }
    }
}