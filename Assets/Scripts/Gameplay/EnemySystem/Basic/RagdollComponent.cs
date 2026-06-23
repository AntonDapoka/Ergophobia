using UnityEngine;

public class RagdollComponent : MonoBehaviour
{
    private Rigidbody[] boneRigidbodies;
    // 【新增】：存储所有骨骼碰撞体
    private Collider[] boneColliders;
    private Animator animator;

    private Collider mainCollider;
    private Rigidbody mainRigidbody;

    private void Awake()
    {
        boneRigidbodies = GetComponentsInChildren<Rigidbody>();
        // 【新增】：获取所有碰撞体
        boneColliders = GetComponentsInChildren<Collider>();

        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        mainRigidbody = GetComponent<Rigidbody>();

        DisableRagdoll();
    }

    public void DisableRagdoll()
    {
        if (animator != null) animator.enabled = true;

        foreach (var rb in boneRigidbodies)
        {
            if (rb != mainRigidbody) rb.isKinematic = true;
        }

        // 【新增】：活着的时候，把骨骼碰撞体设为 Trigger，消除物理阻挡
        foreach (var col in boneColliders)
        {
            if (col != mainCollider)
            {
                col.isTrigger = true;
            }
        }
    }

    public void EnableRagdoll()
    {
        if (animator != null) animator.enabled = false;
        if (mainCollider != null) mainCollider.enabled = false;
        if (mainRigidbody != null) mainRigidbody.isKinematic = true;

        foreach (var rb in boneRigidbodies)
        {
            if (rb != mainRigidbody)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        // 【新增】：死了之后，恢复骨骼碰撞体的物理碰撞，让尸体能砸在地上
        foreach (var col in boneColliders)
        {
            if (col != mainCollider)
            {
                col.isTrigger = false;
            }
        }
    }

    // ... ApplyImpact 方法保持不变 ...
}