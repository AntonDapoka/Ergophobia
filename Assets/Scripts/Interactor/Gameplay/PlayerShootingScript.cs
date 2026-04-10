using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingScript : MonoBehaviour
{

    [SerializeField] private GameObject prefabBulletBasic;
    [SerializeField] private GameObject prefabBulletMighty;
    [SerializeField] private GameObject prefabBulletRandom;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform aimTarget; // цель, в которую стреляем
    [SerializeField] private float shootInterval = 0.2f; 


    private bool isShooting = false;

    /*private void Update()
    {
        if (isShooting == false && Input.GetMouseButtonDown(0))
        {
            isShooting = true;
            StartCoroutine(ShootingLoop());
        }
        else if (isShooting == true && Input.GetMouseButtonDown(0))
        {
            isShooting = false;
        }
    }

    private IEnumerator ShootingLoop()
    {
        while (isShooting)
        {
            ShootBullet();
            yield return new WaitForSeconds(shootInterval);
        }
    }*/

    public void ShootBullet(BlockType blockType)
    {

        GameObject prefabToSpawn;

        switch (blockType)
        {
            case BlockType.BulletFire:
                prefabToSpawn = prefabBulletMighty;
                break;
            case BlockType.BulletRandom:
                if (Random.Range(0, 2) == 0)
                    prefabToSpawn = prefabBulletMighty;  // как BulletFire
                else
                    prefabToSpawn = prefabBulletBasic;
                break;
            case BlockType.BulletBasic:
                prefabToSpawn = prefabBulletBasic;
                break;
            default:
                prefabToSpawn = null ;
                break;
        }

        if (prefabToSpawn == null)
        {
            //Debug.LogError($"Prefab for blockType {blockType} is not assigned!");
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