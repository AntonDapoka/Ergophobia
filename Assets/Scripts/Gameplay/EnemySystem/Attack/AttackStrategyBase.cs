using UnityEngine;
public abstract class AttackStrategyBase : MonoBehaviour
{
    public abstract bool IsAttacking { get; protected set; }
    public abstract void ExecuteAttack(float damage, Transform target);
}