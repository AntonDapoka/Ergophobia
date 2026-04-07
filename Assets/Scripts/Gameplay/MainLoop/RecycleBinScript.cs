using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RecycleBinScript : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject highlightEffect; // Опционально: визуальный эффект при наведении

    public void OnDrop(PointerEventData eventData)
    {
        DraggableBlockScript block = eventData.pointerDrag?.GetComponent<DraggableBlockScript>();
        if (block != null)
        {
            Destroy(block.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(false);
    }
}