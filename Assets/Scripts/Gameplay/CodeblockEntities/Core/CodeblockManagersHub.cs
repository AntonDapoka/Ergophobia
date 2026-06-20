using System.Collections.Generic;
using UnityEngine;

namespace CodeblockEntities
{
    public class CodeblockManagersHub : MonoBehaviour
    {
        static CodeblockManagersHub _instance;
        public static CodeblockManagersHub Instance => _instance;

        [SerializeField] private EnemyCodeblockManager enemyManager;
        [SerializeField] private PlayerCodeblockManager playerManager;
        [SerializeField] private BulletCodeblockManager bulletManager;

        private readonly Dictionary<CodeblockEntityType, ICodeblockEntityManager> _managers = new();

        private void Awake()
        {
            _instance = this;

            RegisterManager(enemyManager);
            RegisterManager(playerManager);
            RegisterManager(bulletManager);
        }

        private void RegisterManager(ICodeblockEntityManager manager)
        {
            if (manager == null) return;
            _managers[manager.EntityType] = manager;
        }

        public ICodeblockEntityManager GetManager(CodeblockEntityType type)
        {
            _managers.TryGetValue(type, out var manager);
            return manager;
        }

        public void RefreshAll()
        {
            foreach (var manager in _managers.Values) manager?.Refresh();
        }
    }
}
