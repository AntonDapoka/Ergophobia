using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleAutoCameraOrbit : MonoBehaviour
{
    [Header("自动旋转设置")]
    [Tooltip("摄像机围绕旋转的中心点空物体")]
    public Transform rotationTarget;

    [Tooltip("水平旋转速度（度/秒）\n正数=顺时针旋转\n负数=逆时针旋转\n0=停止旋转")]
    public float rotationSpeed = 30f;

    // 摄像机与目标的固定距离
    private float fixedDistance;

    void Start()
    {
        // 自动容错：如果没有指定目标，在原点创建一个
        if (rotationTarget == null)
        {
            Debug.LogWarning("未指定旋转目标，自动创建在世界原点的空物体");
            GameObject defaultTarget = new GameObject("AutoRotationTarget");
            rotationTarget = defaultTarget.transform;
            rotationTarget.position = Vector3.zero;
        }

        // 记录摄像机初始位置与目标的距离，全程保持不变
        fixedDistance = Vector3.Distance(transform.position, rotationTarget.position);
    }

    void LateUpdate()
    {
        // 安全检查：目标为空时不执行任何操作
        if (rotationTarget == null) return;

        // 只进行水平旋转（Y轴）
        transform.RotateAround(rotationTarget.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}