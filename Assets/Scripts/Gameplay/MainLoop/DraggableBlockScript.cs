using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableBlockScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{


    [SerializeField] private Image image;
    public TextMeshProUGUI textCount;

    [HideInInspector] public BlockScript block;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

    

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void InitializeBlock(BlockScript newBlock)
    {
        block = newBlock;
        image.sprite = newBlock.sprite;
        RefreshCount();
    }

    public void RefreshCount()
    {
        textCount.text = count.ToString();
        bool textActive = count > 1;
        textCount.gameObject.SetActive(textActive);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }
}
