using System.Collections.Generic;
using UnityEngine;

namespace CodeblockEntities
{
    public class EnemyCodeblockManager : MonoBehaviour, ICodeblockEntityManager
    {
        public CodeblockEntityType EntityType => CodeblockEntityType.Enemy;

        [Header("References")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private LevelTransitionManager levelTransitionManager;

        private readonly List<ICodeblockEntity> _entities = new();
        public IReadOnlyList<ICodeblockEntity> GetEntities() => _entities;

        private void Start()
        {
            Refresh();
            SubscribeToEnemySpawner();
            SubscribeToLevelTransitions();
        }

        private void OnEnable()
        {
            SubscribeToEnemySpawner();
            SubscribeToLevelTransitions();
        }

        private void OnDisable()
        {
            UnsubscribeFromEnemySpawner();
            UnsubscribeFromLevelTransitions();
        }

        public void Refresh()
        {
            _entities.Clear();

            if (enemySpawner == null)
            {
                Debug.LogWarning("[EnemyCodeblockManager] EnemySpawner is not assigned.", this);
                return;
            }

            foreach (GameObject enemy in enemySpawner.GetAllLivingEnemies())
            {
                if (enemy != null)
                    _entities.Add(new EnemyCodeblockEntity(enemy));
            }
        }

        private void SubscribeToEnemySpawner()
        {
            if (enemySpawner == null) return;

            enemySpawner.OnEnemySpawned += HandleEnemySpawned;
            enemySpawner.OnEnemyDestroyed += HandleEnemyDestroyed;
            enemySpawner.OnAllEnemiesCleared += HandleAllEnemiesCleared;
        }

        private void UnsubscribeFromEnemySpawner()
        {
            if (enemySpawner == null) return;

            enemySpawner.OnEnemySpawned -= HandleEnemySpawned;
            enemySpawner.OnEnemyDestroyed -= HandleEnemyDestroyed;
            enemySpawner.OnAllEnemiesCleared -= HandleAllEnemiesCleared;
        }

        private void SubscribeToLevelTransitions()
        {
            if (levelTransitionManager != null)
                levelTransitionManager.OnRoomEntered += HandleRoomEntered;
        }

        private void UnsubscribeFromLevelTransitions()
        {
            if (levelTransitionManager != null)
                levelTransitionManager.OnRoomEntered -= HandleRoomEntered;
        }

        private void HandleEnemySpawned(GameObject enemy)
        {
            if (enemy != null)
                _entities.Add(new EnemyCodeblockEntity(enemy));
        }

        private void HandleEnemyDestroyed(GameObject enemy)
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                if (_entities[i].GameObject == enemy)
                {
                    _entities.RemoveAt(i);
                    break;
                }
            }
        }

        private void HandleAllEnemiesCleared()
        {
            _entities.Clear();
        }

        private void HandleRoomEntered(RoomScript room)
        {
            Refresh();
        }
    }
}
