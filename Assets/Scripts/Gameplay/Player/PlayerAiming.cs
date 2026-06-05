using UnityEngine;

public class PlayerAiming : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private GameObject laserPrefab;
    private GameObject currentLaserInstance; 

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    /*[Header("Animation & Audio")]
    public Animator anim;             
    public AudioSource audioSource;   
    public AudioClip shootSound;     */

    public bool isAiming { get; private set; } = false;

    void Update()
    {
        HandleAimingAndShooting();
    }

    void HandleAimingAndShooting()
    {
        if (Input.GetMouseButton(1))
        {
            if (!isAiming)
            {
                isAiming = true;
                if (laserPrefab != null && firePoint != null)
                {
                    currentLaserInstance = Instantiate(laserPrefab, firePoint.position, firePoint.rotation, firePoint);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
        else
        {
            if (isAiming)
            {
                isAiming = false;

                if (currentLaserInstance != null)
                {
                    Destroy(currentLaserInstance);
                }
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            BulletBasicDebug bullet = bulletObj.GetComponent<BulletBasicDebug>();
            if (bullet != null)
            {
                bullet.Initialize(gameObject);
            }
            /*--- 播放动画 ---
            if (anim != null)
            {
                // 假设你的 Animator 中有一个名为 "Shoot" 的 Trigger 参数
                anim.SetTrigger("Shoot");
            }

            // --- 播放音效 ---
            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);*/
        }      
    }  
}
