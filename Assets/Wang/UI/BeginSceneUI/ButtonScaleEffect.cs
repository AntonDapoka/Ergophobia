using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放设置")]
    [Tooltip("鼠标悬停时的缩放倍率（相对于原始大小）")]
    public float hoverScaleMultiplier = 1.1f;

    [Tooltip("鼠标点击时的缩放倍率（相对于原始大小）")]
    public float clickScaleMultiplier = 0.9f;

    [Header("动画设置")]
    [Tooltip("缩放过渡速度，数值越大过渡越快")]
    public float transitionSpeed = 10f;

    // 自动获取的原始缩放值
    private Vector3 originalScale;
    private Vector3 targetScale;
    private RectTransform rectTransform;

    void Awake()
    {
        // 获取自身的RectTransform组件
        rectTransform = GetComponent<RectTransform>();

        // 自动获取按钮启动时的原始缩放值
        originalScale = rectTransform.localScale;

        // 初始化目标缩放为原始大小
        targetScale = originalScale;
    }

    void Update()
    {
        // 平滑过渡到目标缩放
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, transitionSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 鼠标进入按钮区域
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 只有在没有按下鼠标时才切换到悬停状态
        if (!Input.GetMouseButton(0))
        {
            targetScale = originalScale * hoverScaleMultiplier;
        }
    }

    /// <summary>
    /// 鼠标离开按钮区域
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 离开按钮时回到原始大小
        targetScale = originalScale;
    }

    /// <summary>
    /// 鼠标按下按钮
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 按下时缩小
        targetScale = originalScale * clickScaleMultiplier;
    }

    /// <summary>
    /// 鼠标松开按钮
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // 无论鼠标是否还在按钮上，松开后立即恢复到原始大小
        targetScale = originalScale;
    }

    /// <summary>
    /// 外部调用：强制重置按钮到原始大小
    /// </summary>
    public void ResetToOriginalScale()
    {
        targetScale = originalScale;
        rectTransform.localScale = originalScale;
    }
}