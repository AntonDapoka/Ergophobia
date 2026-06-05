using UnityEngine;

public class PatrolState : IState
{
    private EnemyController enemy;

    public PatrolState(EnemyController enemy) { this.enemy = enemy; }

    public void Enter()
    {
        if (!enemy.PatrolStrategy.HasRoute)
            enemy.Movement.Stop();
    }

    public void Update() { }
    public void Exit() { }
}

public class ChaseState : IState
{
    private EnemyController enemy;
    private Transform target;

    public ChaseState(EnemyController enemy, Transform target)
    {
        this.enemy = enemy;
        this.target = target;
    }

    public void Enter() { }

    public void Update()
    {
        if (target == null) return;

        enemy.Movement.MoveTo(target.position, enemy.Stats.chaseSpeed);

        float distance = Vector3.Distance(enemy.transform.position, target.position);
        if (distance <= enemy.Stats.attackRange)
        {
            enemy.TransitionToState(new AttackState(enemy, target));
        }
    }

    public void Exit() { enemy.Movement.Stop(); }
}

public class AttackState : IState
{
    private EnemyController enemy;
    private Transform target;
    private float lastAttackTime;

    public AttackState(EnemyController enemy, Transform target)
    {
        this.enemy = enemy;
        this.target = target;
    }

    public void Enter()
    {
        enemy.Movement.Stop();
        lastAttackTime = -enemy.Stats.attackCooldown; // 确保一进入就能攻击
    }

    public void Update()
    {
        if (target == null) return;

        // 【核心修改】：如果正在攻击中，强制停止移动，且跳过后续所有逻辑（不转向、不追击）
        if (enemy.AttackStrategy.IsAttacking)
        {
            enemy.Movement.Stop();
            return;
        }

        // 检查玩家是否逃出攻击范围
        float distance = Vector3.Distance(enemy.transform.position, target.position);
        if (distance > enemy.Stats.attackRange)
        {
            // 只有在非攻击状态下，才能切回追击状态
            enemy.TransitionToState(new ChaseState(enemy, target));
            return;
        }

        // 转向玩家 (仅在攻击间隔的空闲时间转向)
        Vector3 direction = (target.position - enemy.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        }

        // 执行攻击 (受冷却时间限制)
        if (Time.time >= lastAttackTime + enemy.Stats.attackCooldown)
        {
            enemy.AttackStrategy.ExecuteAttack(enemy.Stats.attackDamage, target);
            lastAttackTime = Time.time;
        }
    }

    public void Exit() { }
}