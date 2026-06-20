using System.Collections.Generic;
using UnityEngine;

namespace CodeblockEntities
{
    public class EnemyCodeblockManager : MonoBehaviour, ICodeblockEntityManager
    {
        public CodeblockEntityType EntityType => CodeblockEntityType.Enemy;
        [SerializeField] private LevelTransitionManager levelTransitionManager;
        [SerializeField] private float refreshInterval = 0.5f;

        private readonly List<ICodeblockEntity> _entities = new();
        public IReadOnlyList<ICodeblockEntity> GetEntities() => _entities;
        
        private float _timer;

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

            var markers = FindObjectsByType<EnemyMarker>(FindObjectsSortMode.None);
            foreach (var marker in markers)
            {
                if (marker != null && marker.gameObject != null) _entities.Add(new EnemyCodeblockEntity(marker.gameObject));
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
