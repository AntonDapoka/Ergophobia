using System.Collections;
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
   
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

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
        //Range of the explosion
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)) continue;

            if (!canDamageAllies)
            {
                bool isTargetEnemy = hit.GetComponentInParent<EnemyMarker>() != null;
                if (isTargetEnemy) continue;
            }

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform.position);

                GetComponent<EnemyAudio>().PlayExplosion();
                Rigidbody rb = hit.GetComponentInParent<Rigidbody>();

                if (rb != null && !rb.isKinematic)
                {
                    // 计算从爆炸中心到受击者的方向
                    Vector3 pushDir = hit.transform.position - transform.position;

                    // 【核心修改】：将 Y 轴强制设为 0，确保力完全在水平面上
                    pushDir.y = 0;

                    // 归一化向量，保证无论距离多远，方向向量的长度都是 1
                    pushDir.Normalize();

                    // 使用 AddForce 施加瞬间冲力 (Impulse)
                    rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
                }               

                Debug.Log($"<color=red>The explosion hit {hit.name}，taking a damage of {damage} </color>");
            }
        }

        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

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

    //Debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, cancelDistance);
    }
}