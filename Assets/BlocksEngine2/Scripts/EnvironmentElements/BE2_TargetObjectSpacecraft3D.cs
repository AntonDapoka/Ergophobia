using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_BlocksEngine2.Environment
{
    public class BE2_TargetObjectSpacecraft3D : BE2_TargetObject
    {
        [SerializeField] private GameObject _bullet;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimTarget;

        private PlayerAudio playerAudio;

        public new Transform Transform => transform;

        // Centralized bullet tracking so managers (e.g. BulletCodeblockManager) can
        // react to block-spawned bullets without expensive FindObjectsByType calls.
        private static readonly List<BulletBehaviourScript> activeBullets = new();
        public static IReadOnlyList<BulletBehaviourScript> ActiveBullets => activeBullets;

        public static event Action<BulletBehaviourScript> OnBulletSpawned;
        public static event Action<BulletBehaviourScript> OnBulletDestroyed;

        void Awake()
        {
            playerAudio = GetComponent<PlayerAudio>();
            // v2.6 - changed way to find "bullet" child of Target Object
            foreach (Transform child in transform)
            {
                if (child.name == "Bullet")
                    _bullet = child.gameObject;
            }
        }

        public void Shoot()
        {
            ShootBullet(_bullet);
        }

        public void ShootBullet(GameObject prefabToSpawn)
        {
            if (prefabToSpawn == null)
            {
                return;
            }

            // Only allow shooting prefabs that carry the expected bullet behaviour
            if (prefabToSpawn.GetComponent<BulletBehaviourScript>() == null)
            {
                Debug.LogWarning($"[SpacecraftShoot] Prefab '{prefabToSpawn.name}' does not have a BulletBehaviourScript. Skipping shot.");
                return;
            }

            Vector3 direction = (aimTarget.position - firePoint.position).normalized;
            if (playerAudio != null) playerAudio.PlayAttack();
            GameObject bullet = Instantiate(prefabToSpawn, firePoint.position, Quaternion.identity);
           
            if (bullet.TryGetComponent<BulletBehaviourScript>(out var bulletScript))
            {
                bulletScript.SetDirection(gameObject, direction);
                RegisterBullet(bulletScript);
            }
            else
            {
                Debug.LogWarning($"[SpacecraftShoot] Spawned bullet '{bullet.name}' is missing BulletBehaviourScript.");
            }
        }

        private static void RegisterBullet(BulletBehaviourScript bullet)
        {
            if (bullet == null) return;
            activeBullets.Add(bullet);
            OnBulletSpawned?.Invoke(bullet);
        }

        public static void NotifyBulletDestroyed(BulletBehaviourScript bullet)
        {
            if (bullet == null) return;
            activeBullets.Remove(bullet);
            OnBulletDestroyed?.Invoke(bullet);
        }

        public static void ClearActiveBullets()
        {
            foreach (var bullet in activeBullets)
            {
                if (bullet != null)
                    UnityEngine.Object.Destroy(bullet.gameObject);
            }
            activeBullets.Clear();
        }
    }
}
