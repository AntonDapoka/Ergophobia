using UnityEngine;

namespace CodeblockEntities
{
    public class BulletCodeblockEntity : ICodeblockEntity
    {
        readonly GameObject _gameObject;
        readonly BulletBehaviourScript _bullet;

        public GameObject GameObject => _gameObject;

        public BulletCodeblockEntity(GameObject gameObject)
        {
            _gameObject = gameObject;
            _bullet = gameObject.GetComponent<BulletBehaviourScript>();
        }

        public void ModifyHealth(float amount)
        {
            if (_bullet == null) return;
            _bullet.LifeDuration = Mathf.Max(0.1f, _bullet.LifeDuration + amount);
        }

        public void ModifySpeed(float amount)
        {
            if (_bullet == null) return;
            _bullet.Speed = Mathf.Max(0, _bullet.Speed + amount);
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
