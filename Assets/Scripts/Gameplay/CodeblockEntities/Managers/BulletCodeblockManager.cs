using System;
using System.Collections.Generic;
using UnityEngine;
using MG_BlocksEngine2.Environment;

namespace CodeblockEntities
{
    public class BulletCodeblockManager : MonoBehaviour, ICodeblockEntityManager
    {
        public CodeblockEntityType EntityType => CodeblockEntityType.Bullet;

        [Header("References")]
        [Tooltip("Used to clear bullet tracking when a new level is generated." +
                 " Any MonoBehaviour implementing ILevelGenerator is valid.")]
        [SerializeField] private MonoBehaviour levelGeneratorSource;
        [SerializeField] private LevelTransitionManager levelTransitionManager;

        private ILevelGenerator levelGenerator;
        private readonly List<ICodeblockEntity> _entities = new();
        public IReadOnlyList<ICodeblockEntity> GetEntities() => _entities;

        private void Awake()
        {
            levelGenerator = levelGeneratorSource as ILevelGenerator;

            if (levelGenerator == null && levelGeneratorSource != null)
                Debug.LogError($"[BulletCodeblockManager] '{levelGeneratorSource.name}' does not implement {nameof(ILevelGenerator)}.", this);
        }

        private void Start()
        {
            Refresh();
            SubscribeToBulletEvents();
            SubscribeToLevelGeneration();
            SubscribeToLevelTransitions();
        }

        private void OnEnable()
        {
            SubscribeToBulletEvents();
            SubscribeToLevelGeneration();
            SubscribeToLevelTransitions();
        }

        private void OnDisable()
        {
            UnsubscribeFromBulletEvents();
            UnsubscribeFromLevelGeneration();
            UnsubscribeFromLevelTransitions();
        }

        public void Refresh()
        {
            _entities.Clear();

            foreach (BulletBehaviourScript bullet in BE2_TargetObjectSpacecraft3D.ActiveBullets)
            {
                if (bullet != null && bullet.gameObject != null)
                    _entities.Add(new BulletCodeblockEntity(bullet.gameObject));
            }
        }

        private void SubscribeToBulletEvents()
        {
            BE2_TargetObjectSpacecraft3D.OnBulletSpawned += HandleBulletSpawned;
            BE2_TargetObjectSpacecraft3D.OnBulletDestroyed += HandleBulletDestroyed;
        }

        private void UnsubscribeFromBulletEvents()
        {
            BE2_TargetObjectSpacecraft3D.OnBulletSpawned -= HandleBulletSpawned;
            BE2_TargetObjectSpacecraft3D.OnBulletDestroyed -= HandleBulletDestroyed;
        }

        private void SubscribeToLevelGeneration()
        {
            if (levelGenerator != null)
                levelGenerator.OnLevelGenerated += HandleLevelGenerated;
        }

        private void UnsubscribeFromLevelGeneration()
        {
            if (levelGenerator != null)
                levelGenerator.OnLevelGenerated -= HandleLevelGenerated;
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

        private void HandleBulletSpawned(BulletBehaviourScript bullet)
        {
            if (bullet != null && bullet.gameObject != null)
                _entities.Add(new BulletCodeblockEntity(bullet.gameObject));
        }

        private void HandleBulletDestroyed(BulletBehaviourScript bullet)
        {
            if (bullet == null) return;

            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                if (_entities[i].GameObject == bullet.gameObject)
                {
                    _entities.RemoveAt(i);
                    break;
                }
            }
        }

        private void HandleLevelGenerated(List<RoomScript> rooms)
        {
            _entities.Clear();
            BE2_TargetObjectSpacecraft3D.ClearActiveBullets();
        }

        private void HandleRoomEntered(RoomScript room)
        {
            Refresh();
        }
    }
}
