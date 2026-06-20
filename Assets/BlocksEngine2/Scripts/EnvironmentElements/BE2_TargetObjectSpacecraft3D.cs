using System.Collections;
using UnityEngine;

namespace MG_BlocksEngine2.Environment
{
    public class BE2_TargetObjectSpacecraft3D : BE2_TargetObject
    {
        [SerializeField] private GameObject _bullet;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimTarget;

        public new Transform Transform => transform;

        void Awake()
        {
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
            GameObject bullet = Instantiate(prefabToSpawn, firePoint.position, Quaternion.identity);

            if (bullet.TryGetComponent<BulletBehaviourScript>(out var bulletScript))
            {
                bulletScript.SetDirection(gameObject, direction);
            }
            else
            {
                Debug.LogWarning($"[SpacecraftShoot] Spawned bullet '{bullet.name}' is missing BulletBehaviourScript.");
            }
        }
    }
}
