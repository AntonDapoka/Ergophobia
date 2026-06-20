using System.Collections.Generic;
using UnityEngine;

namespace CodeblockEntities
{
    public class PlayerCodeblockManager : MonoBehaviour, ICodeblockEntityManager
    {
        public CodeblockEntityType EntityType => CodeblockEntityType.Player;
        [SerializeField] private LevelTransitionManager levelTransitionManager;
        [SerializeField] private PlayerMarker playerMarker;

        private readonly List<ICodeblockEntity> _entities = new();
        public IReadOnlyList<ICodeblockEntity> GetEntities() => _entities;

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

        public void Refresh()
        {
            _entities.Clear();
            if (playerMarker != null && playerMarker.gameObject != null) _entities.Add(new PlayerCodeblockEntity(playerMarker.gameObject));
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
