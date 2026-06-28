using System.Collections;
using UnityEngine;

/// <summary>
/// 通用的受击闪烁组件。
/// 只要物体上挂载了 HealthComponent 或 PlayerHealthDebug，即可自动工作。
/// </summary>
public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private Renderer[] renderers;
    private Coroutine flashCoroutine;

    // 缓存组件引用，用于注销事件
    private HealthComponent enemyHealth;
    private PlayerHealthDebug playerHealth;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // 尝试获取敌人或玩家的生命值组件
        enemyHealth = GetComponent<HealthComponent>();
        playerHealth = GetComponent<PlayerHealthDebug>();

        if (enemyHealth == null && playerHealth == null)
        {
            Debug.LogWarning($"[DamageFlash] {gameObject.name} 身上没有找到任何生命值组件！");
        }
    }

    private void OnEnable()
    {
        // 谁存在，就监听谁的 OnHit 事件
        if (enemyHealth != null) enemyHealth.OnHit += HandleHitFlash;
        if (playerHealth != null) playerHealth.OnHit += HandleHitFlash;
    }

    private void OnDisable()
    {
        // 注销事件，防止内存泄漏
        if (enemyHealth != null) enemyHealth.OnHit -= HandleHitFlash;
        if (playerHealth != null) playerHealth.OnHit -= HandleHitFlash;

        ResetColor();
    }

    private void HandleHitFlash()
    {
        if (renderers == null || renderers.Length == 0) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(propBlock);
            // 兼容 URP/HDRP 和 Built-in 材质的颜色属性名
            propBlock.SetColor("_BaseColor", flashColor);
            propBlock.SetColor("_Color", flashColor);
            r.SetPropertyBlock(propBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        ResetColor();
    }

    private void ResetColor()
    {
        if (renderers == null) return;

        foreach (Renderer r in renderers)
        {
            r.SetPropertyBlock(null); // 清除 PropertyBlock，恢复材质原本颜色
        }
    }
}