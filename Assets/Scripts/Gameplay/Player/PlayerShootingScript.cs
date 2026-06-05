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

        BulletBehaivourScript bulletScript = bullet.GetComponent<BulletBehaivourScript>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
        }
        else
        {
            Debug.Log("Problemssss");
        }
    }
}