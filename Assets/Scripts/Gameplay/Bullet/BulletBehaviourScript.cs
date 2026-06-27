using System.Collections;
using UnityEngine;
using MG_BlocksEngine2.Environment;

[RequireComponent(typeof(Rigidbody))]
public class BulletBehaviourScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeDuration = 1f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private AudioSource audiosource;
    [SerializeField] private AudioClip hitSFX;

    public float Speed
    {
        get => speed;
        set
        {
            speed = value;
            if (rb != null && direction != Vector3.zero)
                rb.linearVelocity = direction * speed;
        }
    }

    public float LifeDuration
    {
        get => lifeDuration;
        set
        {
            lifeDuration = value;
            RestartSelfDestruct();
        }
    }

    private void RestartSelfDestruct()
    {
        StopAllCoroutines();
        StartCoroutine(SelfDestruct());
    }

    private GameObject owner;

    private Rigidbody rb;
    private Vector3 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    
    public void SetDirection(GameObject shooter, Vector3 dir)
    {
        owner = shooter;
        dir.y = 0f;
        direction = dir.normalized;

        rb.linearVelocity = direction * speed;

        StartCoroutine(SelfDestruct());
    }

    private void FixedUpdate()
    {
        Vector3 pos = rb.position;
        pos.y = 1f;
        rb.position = pos;
    }

    private void OnDestroy()
    {
        BE2_TargetObjectSpacecraft3D.NotifyBulletDestroyed(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;

        if (owner != null && (hitObject == owner || hitObject.transform.IsChildOf(owner.transform))) return;

        IDamageable damageable = hitObject.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            Vector3 sourcePos = owner != null ? owner.transform.position : transform.position;
            damageable.TakeDamage(damage, sourcePos);

            Debug.Log($"<color=cyan>Bullet hits {hitObject.name} and deals {damage} damage</color>");
        }
        if (hitSFX != null)
        {
            audiosource.pitch = Random.Range(1.2f, 1.8f);
            AudioSource.PlayClipAtPoint(hitSFX, collision.contacts[0].point, 1.0f);
        }
        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate( explosionVFX, collision.contacts[0].point, Quaternion.identity);
            Destroy(vfx, 2f);
        }
        if (hitObject.GetComponent<WallMarker>() != null)
        {
            Vector3 normal = collision.contacts[0].normal;
            normal.y = 0f;

            direction = Vector3.Reflect(direction, normal).normalized;
            rb.linearVelocity = direction * speed;
        }
        else Destroy(gameObject);
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(lifeDuration);
        Destroy(gameObject);
    }
}