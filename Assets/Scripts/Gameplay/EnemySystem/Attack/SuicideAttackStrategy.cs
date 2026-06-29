using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class SuicideAttackStrategy : AttackStrategyBase
{
    [Header("Explosion Settings")]
    [SerializeField] private float fuseTime = 1.5f;
    [SerializeField] private float explosionRadius;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private bool canDamageAllies = true;
    [SerializeField] private float pushForce;

    [Header("Cancel Settings")]
    [SerializeField] private float cancelableTime;
    [SerializeField] private float cancelDistance;

    [Header("Warning Circle ")]
    [SerializeField] private LineRenderer warningCircle;
    [SerializeField] private int circleSegments = 50;

    private IDamageable self;
    private Animator ani;

    void Start()
    {
        self = GetComponentInParent<IDamageable>();
        ani = GetComponentInParent<Animator>();

        if (warningCircle != null)
        {
            warningCircle.enabled = false;
            SetupWarningCircle();
        }
    }

    public override bool IsAttacking { get; protected set; }

    public override void ExecuteAttack(float damage, Transform target)
    {
        if (!IsAttacking)
        {
            StartCoroutine(DetonationRoutine(damage, target));
        }
    }

    private IEnumerator DetonationRoutine(float damage, Transform target)
    {
        IsAttacking = true;

        if (ani != null) ani.SetTrigger("Attack");
        GetComponent<EnemyAudio>().PlayAttack();
        if (warningCircle != null) warningCircle.enabled = true;

        float timer = 0f;

        while (timer < fuseTime)
        {
            if (timer < cancelableTime && target != null)
            {
                Vector3 myPos2D = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 targetPos2D = new Vector3(target.position.x, 0, target.position.z);
                float distanceToTarget = Vector3.Distance(myPos2D, targetPos2D);

                if (distanceToTarget > cancelDistance)
                {
                    CancelDetonation();
                    yield break;
                }
            }

            timer += Time.deltaTime;
            float percent = timer / fuseTime;

            if (warningCircle != null)
            {
                Color circleColor = warningCircle.startColor;
                circleColor.a = Mathf.Lerp(0.5f, 1f, percent);
                warningCircle.startColor = circleColor;
                warningCircle.endColor = circleColor;
            }

            yield return null;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius + 2f);

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        Vector3 explosionCenter2D = new Vector3(transform.position.x, 0, transform.position.z);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)) continue;
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null) continue;

            if (damagedTargets.Contains(damageable)) continue;
            Transform targetRoot = (damageable as MonoBehaviour).transform;

            Vector3 targetCenter2D = new Vector3(targetRoot.position.x, 0, targetRoot.position.z);

            float distance2D = Vector3.Distance(explosionCenter2D, targetCenter2D);
            if (distance2D > explosionRadius) continue;

            if (!canDamageAllies)
            {
                bool isTargetEnemy = targetRoot.GetComponent<EnemyMarker>() != null;
                if (isTargetEnemy) continue;
            }

            damageable.TakeDamage(damage, transform.position);
            damagedTargets.Add(damageable); 

            // »÷ÍËÂß¼­
            Rigidbody rb = targetRoot.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                Vector3 pushDir = targetRoot.position - transform.position;
                pushDir.y = 0;
                pushDir.Normalize();
                rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
            }

            Debug.Log($"<color=red>The explosion hit {targetRoot.name}, taking a damage of {damage} </color>");
        }

        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
        GetComponent<EnemyAudio>().PlayExplosion();

        if (self != null) self.TakeDamage(float.MaxValue, transform.position);
    }

    private void CancelDetonation()
    {
        Debug.Log("<color=yellow>The player has escaped the range, self-detonation canceled, resuming chase!</color>");

        if (warningCircle != null) warningCircle.enabled = false;
        if (ani != null) ani.ResetTrigger("Attack");

        IsAttacking = false;
    }

    private void SetupWarningCircle()
    {
        warningCircle.useWorldSpace = false;
        warningCircle.positionCount = circleSegments + 1;

        float angle = 0f;
        for (int i = 0; i < (circleSegments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * explosionRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * explosionRadius;
            warningCircle.SetPosition(i, new Vector3(x, 0.1f, z));
            angle += (360f / circleSegments);
        }
    }
}