using System;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
    void TakeDamage(float damage, Vector3 damageSourcePosition);
    event Action OnDeath;
}

public interface IMovable
{
    void MoveTo(Vector3 destination, float speed);
    void Stop();
    bool IsAtDestination();
}

public interface IPatrolStrategy
{
    bool HasRoute { get; }
    Vector3 GetNextWaypoint();
}

public interface IState
{
    void Enter();
    void Update();
    void Exit();
}
