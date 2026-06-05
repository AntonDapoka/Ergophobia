using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletBasicDebug : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Visual Effects")]
    public GameObject explosionVFX;

    private Rigidbody rb;
    private GameObject owner;

    public void Initialize(GameObject shooter)
    {
        owner = shooter;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);

        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        if (owner != null)
        {
            bool isShooterEnemy = owner.GetComponent<EnemyMarker>() != null;
            bool isTargetEnemy = other.GetComponentInParent<EnemyMarker>() != null;

            if (isShooterEnemy && isTargetEnemy)
            {
                return;
            }
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            Vector3 sourcePos = owner != null ? owner.transform.position : transform.position;

            damageable.TakeDamage(damage, sourcePos);
            Debug.Log($"<color=cyan>Bullet hits {other.name} and takes a damage of {damage} </color>");
        }

        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
        Destroy(gameObject);
    }
}