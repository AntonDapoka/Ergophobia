using System.Collections.Generic;
using UnityEngine;

namespace CodeblockEntities
{
    public class BulletCodeblockManager : MonoBehaviour, ICodeblockEntityManager
    {
        public CodeblockEntityType EntityType => CodeblockEntityType.Bullet;
        [SerializeField] private LevelTransitionManager levelTransitionManager;
        [SerializeField] private float refreshInterval = 0.1f;

        private readonly List<ICodeblockEntity> _entities = new();
        public IReadOnlyList<ICodeblockEntity> GetEntities() => _entities;
        
        float _timer;

        private void Start()
        {
            Refresh();
            SubscribeToLevelTransitions();
        }

        private void OnEnable()
        {
            SubscribeToLevelTransitions();
        }

        private void OnDisable()
        {
            UnsubscribeFromLevelTransitions();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= refreshInterval)
            {
                _timer = 0f;
                Refresh();
            }
        }

        public void Refresh()
        {
            _entities.Clear();

            var bullets = FindObjectsByType<BulletBehaviourScript>(FindObjectsSortMode.None);
            foreach (var bullet in bullets)
            {
                if (bullet != null && bullet.gameObject != null)
                    _entities.Add(new BulletCodeblockEntity(bullet.gameObject));
            }
        }

        private void SubscribeToLevelTransitions()
        {
            if (levelTransitionManager != null) levelTransitionManager.OnRoomEntered += HandleRoomEntered;
        }

        private void UnsubscribeFromLevelTransitions()
        {
            if (levelTransitionManager != null) levelTransitionManager.OnRoomEntered -= HandleRoomEntered;
        }

        private void HandleRoomEntered(RoomScript room)
        {
            Refresh();
        }
    }
}
