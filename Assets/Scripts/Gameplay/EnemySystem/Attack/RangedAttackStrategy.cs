using System.Collections;
using UnityEngine;

public class RangedAttackStrategy : AttackStrategyBase
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;      
    [SerializeField] private GameObject bulletPrefab;  
    [SerializeField] private GameObject aimVFXPrefab;  

    [Header("Attack Settings")]
    [SerializeField] private float aimDuration; 
    [SerializeField] private float fireRecovery; 
    [SerializeField] private float aimTurnSpeed; 
    [SerializeField] private float aniFireFrame = 0.5f; 

    public override bool IsAttacking { get; protected set; }

    private GameObject currentAimVFXInstance;

    public override void ExecuteAttack(float damage, Transform target)
    {
        if (!IsAttacking)
        {
            StartCoroutine(AttackRoutine(damage, target));
        }
    }

    private IEnumerator AttackRoutine(float damage, Transform target)
    {
        IsAttacking = true;

        if (aimVFXPrefab != null && firePoint != null)
        {
            currentAimVFXInstance = Instantiate(aimVFXPrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        float aimTimer = 0f;

        while (aimTimer < aimDuration)
        {
            if (target == null) break;

            Vector3 targetChestPos = target.position + Vector3.up * 1.2f;

            Vector3 bodyLookDir = target.position - transform.position;
            bodyLookDir.y = 0;
            if (bodyLookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(bodyLookDir), Time.deltaTime * aimTurnSpeed);
            }


            if (currentAimVFXInstance != null)
            {
                currentAimVFXInstance.transform.LookAt(targetChestPos);
            }

            aimTimer += Time.deltaTime;
            yield return null;
        }

        if (currentAimVFXInstance != null)
        {
            Destroy(currentAimVFXInstance);
        }

        if (target != null)
        {
            Vector3 finalLookDir = target.position - transform.position;
            finalLookDir.y = 0;
            if (finalLookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(finalLookDir);
            }
        }

        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(aniFireFrame); 
        if (target != null && bulletPrefab != null)
        {
            Vector3 targetChestPos = target.position + Vector3.up * 1.2f;
            Vector3 shootDirection = (targetChestPos - firePoint.position).normalized;

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
            GetComponent<EnemyAudio>().PlayAttack();
            BulletBasicDebug bullet = bulletObj.GetComponent<BulletBasicDebug>();
            if (bullet != null)
            {
                bullet.Initialize(gameObject);
            }
        }

        yield return new WaitForSeconds(fireRecovery);

        IsAttacking = false;
    }

    private void OnDisable()
    {
        if (currentAimVFXInstance != null)
        {
            Destroy(currentAimVFXInstance);
        }
        IsAttacking = false;
    }
}