using UnityEngine;

// ==========================================
// 1. Ñ²Âß×´Ì¬
// ==========================================
public class PatrolState : IState
{
    private EnemyController enemy;

    public PatrolState(EnemyController enemy) { this.enemy = enemy; }

    public void Enter()
    {
        if (!enemy.PatrolStrategy.HasRoute)
            enemy.Movement.Stop();
    }

    public void Update()
    {
        // Patrol logic?
    }

    public void Exit() { }
}

// ==========================================
// 2. ×·»÷×´Ì¬
// ==========================================
public class ChaseState : IState
{
    private EnemyController enemy;
    private Transform target;
    private float turnThreshold = 15f; 

    public ChaseState(EnemyController enemy, Transform target)
    {
        this.enemy = enemy;
        this.target = target;
    }

    public void Enter() { }

    public void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - enemy.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            float angle = Vector3.Angle(enemy.transform.forward, direction);

            if (angle > turnThreshold)
            {
                enemy.Movement.Stop();
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 12f);
            }
            else
            {
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 12f);
                enemy.Movement.MoveTo(target.position, enemy.Stats.chaseSpeed);
            }
        }

        float distance = Vector3.Distance(enemy.transform.position, target.position);

        if (distance <= enemy.Stats.attackRange)
        {
            CombatManager.CombatRole role = enemy.Stats.isRanged ? CombatManager.CombatRole.Ranged : CombatManager.CombatRole.Melee;
            if (CombatManager.Instance.HasToken(enemy.gameObject, role))
            {
                enemy.TransitionToState(new AttackState(enemy, target));
            }
            else
            {
                enemy.TransitionToState(new CombatWaitState(enemy, target, false));
            }
        }
    }

    public void Exit() { enemy.Movement.Stop(); }
}
// ==========================================
// 3. ¹¥»÷×´Ì¬
// ==========================================
public class AttackState : IState
{
    private EnemyController enemy;
    private Transform target;
    private bool hasAttacked = false;
    private bool attackCommandSent = false; // È·±£Ö»´¥·¢Ò»´Î¹¥»÷Ö¸Áî

    public AttackState(EnemyController enemy, Transform target)
    {
        this.enemy = enemy;
        this.target = target;
    }

    public void Enter()
    {
        enemy.Movement.Stop();
        hasAttacked = false;
        attackCommandSent = false;
    }

    public void Update()
    {
        if (target == null) return;
        enemy.Movement.Stop();

        if (enemy.AttackStrategy.IsAttacking)
        {
            hasAttacked = true;
            return;
        }

        if (hasAttacked && !enemy.AttackStrategy.IsAttacking)
        {
            enemy.TransitionToState(new CombatWaitState(enemy, target, true));
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, target.position);
        if (distance > enemy.Stats.attackRange)
        {
            enemy.TransitionToState(new ChaseState(enemy, target));
            return;
        }

        Vector3 direction = (target.position - enemy.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        }
        if (!attackCommandSent)
        {
            enemy.AttackStrategy.ExecuteAttack(enemy.Stats.attackDamage, target);
            attackCommandSent = true;
        }
    }

    public void Exit()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.CombatRole role = enemy.Stats.isRanged ? CombatManager.CombatRole.Ranged : CombatManager.CombatRole.Melee;
            CombatManager.Instance.ReleaseToken(enemy.gameObject, role);
        }
    }
}
// ==========================================
// 4. Õ½¶·µÈ´ý×´Ì¬ (õâ²½°üÎ§ÓëÒ¡ºÅÅÅ¶Ó)
// ==========================================
public class CombatWaitState : IState
{
    private EnemyController enemy;
    private Transform target;

    private float waitDistance;
    private float strafeTimer = 0f;
    private int strafeDirection = 1;
    private float turnThreshold = 20f;

    private bool isWaitingForCooldown;
    private float cooldownTimer;
    private bool hasRegistered = false;

    public CombatWaitState(EnemyController enemy, Transform target, bool justAttacked = false)
    {
        this.enemy = enemy;
        this.target = target;
        this.waitDistance = enemy.Stats.attackRange + 1.0f;

        if (justAttacked)
        {
            isWaitingForCooldown = true;
            cooldownTimer = enemy.Stats.attackCooldown;
        }
    }

    public void Enter()
    {
        strafeDirection = Random.value > 0.5f ? 1 : -1;
        hasRegistered = false;

        if (!isWaitingForCooldown)
        {
            RegisterForLottery();
        }
    }

    private void RegisterForLottery()
    {
        CombatManager.CombatRole role = enemy.Stats.isRanged ? CombatManager.CombatRole.Ranged : CombatManager.CombatRole.Melee;
        CombatManager.Instance.RegisterForToken(enemy.gameObject, role);
        hasRegistered = true;
    }

    public void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(enemy.transform.position, target.position);

        if (distance > waitDistance + 2.0f)
        {
            enemy.TransitionToState(new ChaseState(enemy, target));
            return;
        }

        Vector3 lookPos = target.position - enemy.transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
        {
            float angle = Vector3.Angle(enemy.transform.forward, lookPos);
            if (angle > turnThreshold)
            {
                enemy.Movement.Stop();
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 12f);
            }
            else
            {
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 12f);
                strafeTimer += Time.deltaTime * 0.5f * strafeDirection;
                Vector3 offset = new Vector3(Mathf.Sin(strafeTimer), 0, Mathf.Cos(strafeTimer)) * waitDistance;
                enemy.Movement.MoveTo(target.position + offset, enemy.Stats.chaseSpeed * 0.5f);
            }
        }

        if (isWaitingForCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isWaitingForCooldown = false;
                RegisterForLottery(); 
            }
        }
        else if (hasRegistered)
        {
            CombatManager.CombatRole role = enemy.Stats.isRanged ? CombatManager.CombatRole.Ranged : CombatManager.CombatRole.Melee;
            if (CombatManager.Instance.HasToken(enemy.gameObject, role))
            {
                enemy.TransitionToState(new ChaseState(enemy, target));
            }
        }
    }

    public void Exit()
    {
        enemy.Movement.Stop();
        if (CombatManager.Instance != null && hasRegistered)
        {
            CombatManager.CombatRole role = enemy.Stats.isRanged ? CombatManager.CombatRole.Ranged : CombatManager.CombatRole.Melee;
            CombatManager.Instance.UnregisterFromToken(enemy.gameObject, role);
        }
    }
}