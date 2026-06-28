using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging; 

public class PlayerRigController : MonoBehaviour
{
    [Header("Rigging 引用")]
    [Tooltip("拖入层级中的 Rig 2 物体")]
    public Rig upperBodyRig;

    [Header("过渡设置")]
    [Tooltip("IK 开启/关闭的平滑过渡时间，防止手臂瞬间瞬移")]
    public float blendDuration = 0.15f;

    private Coroutine weightCoroutine;

    /// <summary>
    /// 在玩家受击时调用此方法
    /// </summary>
    /// <param name="disableDuration">受击动画的总时长</param>
    public void DisableIKForHit(float disableDuration)
    {
        if (weightCoroutine != null)
        {
            StopCoroutine(weightCoroutine);
        }
        weightCoroutine = StartCoroutine(ToggleRigWeight(disableDuration));
    }

    private IEnumerator ToggleRigWeight(float disableDuration)
    {
        float time = 0;
        float startWeight = upperBodyRig.weight;

        // 1. 快速将权重降为 0 (关闭 IK，让受击动画接管)
        while (time < blendDuration)
        {
            time += Time.deltaTime;
            upperBodyRig.weight = Mathf.Lerp(startWeight, 0f, time / blendDuration);
            yield return null;
        }
        upperBodyRig.weight = 0f;

        // 2. 等待受击动画播放 (减去过渡时间，防止等待过久)
        float waitTime = Mathf.Max(0f, disableDuration - (blendDuration * 2));
        yield return new WaitForSeconds(waitTime);

        // 3. 缓慢将权重恢复为 1 (重新开启 IK，恢复举枪姿势)
        time = 0;
        while (time < blendDuration)
        {
            time += Time.deltaTime;
            upperBodyRig.weight = Mathf.Lerp(0f, 1f, time / blendDuration);
            yield return null;
        }
        upperBodyRig.weight = 1f;
    }
}