using UnityEngine;

namespace CodeblockEntities
{
    public class PlayerCodeblockEntity : ICodeblockEntity
    {
        readonly GameObject _gameObject;
        readonly PlayerHealthDebug _health;
        readonly PlayerMoving _moving;

        public GameObject GameObject => _gameObject;

        public PlayerCodeblockEntity(GameObject gameObject)
        {
            _gameObject = gameObject;
            _health = gameObject.GetComponent<PlayerHealthDebug>();
            _moving = gameObject.GetComponent<PlayerMoving>();
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
            if (_moving == null) return;
            _moving.WalkSpeed = Mathf.Max(0, _moving.WalkSpeed + amount);
            _moving.RunSpeed = Mathf.Max(0, _moving.RunSpeed + amount);
        }

        public void Destroy()
        {
            // Intentionally avoid physically destroying the player; trigger death instead.
            _health?.TakeDamage(float.MaxValue, _gameObject != null ? _gameObject.transform.position : Vector3.zero);
        }

        public void Scale(float amount)
        {
            if (_gameObject == null) return;
            _gameObject.transform.localScale *= Mathf.Max(0.1f, 1f + amount);
        }
    }
}
