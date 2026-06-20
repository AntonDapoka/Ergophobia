using UnityEngine;
using UnityEngine.AI;

namespace CodeblockEntities
{
    public class EnemyCodeblockEntity : ICodeblockEntity
    {
        readonly GameObject _gameObject;
        readonly HealthComponent _health;
        readonly NavMeshAgent _agent;
        readonly EnemyController _controller;

        public GameObject GameObject => _gameObject;

        public EnemyCodeblockEntity(GameObject gameObject)
        {
            _gameObject = gameObject;
            _health = gameObject.GetComponent<HealthComponent>();
            _agent = gameObject.GetComponent<NavMeshAgent>();
            _controller = gameObject.GetComponent<EnemyController>();
        }

        public void ModifyHealth(float amount)
        {
            if (_gameObject == null) return;

            if (amount > 0)
                _health?.Heal(amount);
            else if (amount < 0)
                _health?.TakeDamage(-amount);
        }

        public void ModifySpeed(float amount)
        {
            if (_gameObject == null) return;

            if (_controller != null && _controller.Stats != null)
            {
                _controller.Stats.patrolSpeed = Mathf.Max(0, _controller.Stats.patrolSpeed + amount);
                _controller.Stats.chaseSpeed = Mathf.Max(0, _controller.Stats.chaseSpeed + amount);
            }

            if (_agent != null)
                _agent.speed = Mathf.Max(0, _agent.speed + amount);
        }

        public void Destroy()
        {
            if (_gameObject != null)
                Object.Destroy(_gameObject);
        }

        public void Scale(float amount)
        {
            if (_gameObject == null) return;
            _gameObject.transform.localScale *= Mathf.Max(0.1f, 1f + amount);
        }
    }
}
