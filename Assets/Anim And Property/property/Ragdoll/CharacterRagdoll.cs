using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CharacterRagdoll : MonoBehaviour
{
    private Animator anim;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    // 活着时的主碰撞体和主刚体
    private Collider mainCollider;
    private Rigidbody mainRigidbody;

    [Header("需要联动的组件")]
    // 1. 把你之前写的处理 ALS 移动和鼠标朝向的脚本拖到这里
    // 假设你的移动脚本叫 MyALSController（根据你的实际脚本名修改）
    public MonoBehaviour movementScript;

    // 2. 把负责手部和头部 IK 的 RigBuilder 组件拖到这里
    public RigBuilder rigBuilder;

    private bool isRagdollActive = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        mainRigidbody = GetComponent<Rigidbody>();

        // 如果你没有在 Inspector 里手动拖拽，代码会尝试自动在当前物体上找移动脚本
        // 如果你的移动脚本叫别的名字，请把下面的 "MyALSController" 改掉
        if (movementScript == null) movementScript = GetComponent("MyALSController") as MonoBehaviour;

        // 自动获取 IK 组件
        if (rigBuilder == null) rigBuilder = GetComponent<RigBuilder>();

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        SetRagdollState(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isRagdollActive = !isRagdollActive;
            SetRagdollState(isRagdollActive);
        }
    }

    public void SetRagdollState(bool isDead)
    {
        // A. 控制动画状态机的开关
        if (anim != null) anim.enabled = !isDead;

        // 【关键改动：控制移动与 IK 的开关】
        // 如果死了（isDead = true），脚本和 IK 就要被禁用（enabled = false）
        if (movementScript != null) movementScript.enabled = !isDead;
        if (rigBuilder != null) rigBuilder.enabled = !isDead;

        // B. 遍历并控制所有骨骼上的刚体和碰撞体
        foreach (var rb in ragdollRigidbodies)
        {
            if (rb != mainRigidbody)
            {
                rb.isKinematic = !isDead;
            }
        }

        foreach (var col in ragdollColliders)
        {
            if (col != mainCollider)
            {
                col.enabled = isDead;
            }
        }

        // C. 处理主组件的开关
        if (mainCollider != null) mainCollider.enabled = !isDead;
    }
}