using UnityEngine;

public abstract class EnemyInteractorScript : MonoBehaviour
{
    protected float health;
    protected bool isAbleToDamage = true;
    protected bool isAbleToTakeDamage = true;

    public abstract void Initialize();
}