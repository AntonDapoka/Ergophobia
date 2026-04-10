using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class AssemblerSlotScript : MonoBehaviour, IDropHandler
{
    public int index;
    [SerializeField] private BlockAssembleScript assembler;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableBlockScript block = eventData.pointerDrag.GetComponent<DraggableBlockScript>();

        if (transform.childCount == 0)
        {
            block.parentAfterDrag = transform;
        }

        UpdateCanvasActive(block);

        if (block.originalBlock != null) UpdateCanvasActive(block.originalBlock);
    }

    private void UpdateCanvasActive(DraggableBlockScript block)
    {
        block.image.raycastTarget = true;

        foreach (var img in block.GetComponentsInChildren<Image>())
        {
            if (img.GetComponent<SlotMarker>() != null || img.GetComponent<ShouldBeActiveMarker>() != null)
                img.raycastTarget = true;
            else
                img.raycastTarget = false;
        }

        foreach (var txt in block.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (txt.GetComponent<SlotMarker>() != null || txt.GetComponent<ShouldBeActiveMarker>() != null)
                txt.raycastTarget = true;
            else
                txt.raycastTarget = false;
        }
    }
}
