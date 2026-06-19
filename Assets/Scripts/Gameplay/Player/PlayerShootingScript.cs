using UnityEngine;

public class PlayerShootingScript : MonoBehaviour
{
    [SerializeField] private GameObject prefabBulletBasic;
    [SerializeField] private GameObject prefabBulletMighty;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform aimTarget;

    public void ShootBullet(GameObject prefabToSpawn)
    {

        if (prefabToSpawn == null)
        {
            return;
        }

        Vector3 direction = (aimTarget.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(prefabToSpawn, firePoint.position, Quaternion.identity);

        BulletBehaviourScript bulletScript = bullet.GetComponent<BulletBehaviourScript>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(gameObject, direction);
        }
        else
        {
            Debug.Log("Problemssss");
        }
    }
}