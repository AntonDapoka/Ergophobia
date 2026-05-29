using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.DragDrop;

namespace MG_BlocksEngine2.Environment
{
    /// <summary>
    /// A free-form container for blocks. Blocks can be placed and picked up freely.
    /// No lines, no sublines, no execution, no connections between blocks.
    /// Supports scrolling if content exceeds the visible area.
    /// </summary>
    public class StorageEnvironment : HolderEnvironment
    {
        [Header("Scrolling")]
        [Tooltip("Optional ScrollRect. If null, one will be added automatically.")]
        public ScrollRect scrollRect;
        [Tooltip("The content area used for scrolling. If null, a child named 'Content' will be created.")]
        public RectTransform contentContainer;

        [Header("Visuals")]
        [Tooltip("Background color of the storage panel.")]
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [Tooltip("Optional background sprite (sliced).")]
        public Sprite backgroundSprite;

        GraphicRaycaster _graphicRaycaster;

        protected virtual void Awake()
        {
            _graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
            Debug.Log($"[StorageEnvironment] Awake: {name}, found GraphicRaycaster={(_graphicRaycaster != null ? _graphicRaycaster.name : "NULL")}");
            SetupScrolling();
            SetupVisuals();
        }

        protected virtual void Start()
        {
            if (_graphicRaycaster != null && BE2_DragDropManager.Instance != null)
            {
                BE2_DragDropManager.Instance.Raycaster.AddRaycaster(_graphicRaycaster);
                Debug.Log($"[StorageEnvironment] Start: {name} registered GraphicRaycaster {_graphicRaycaster.name}");
            }
            else
            {
                Debug.LogWarning($"[StorageEnvironment] Start: {name} FAILED to register raycaster. _graphicRaycaster={(_graphicRaycaster != null)}, BE2_DragDropManager.Instance={(BE2_DragDropManager.Instance != null)}");
            }
        }

        protected virtual void OnDisable()
        {
            if (_graphicRaycaster != null && BE2_DragDropManager.Instance != null)
            {
                BE2_DragDropManager.Instance.Raycaster.RemoveRaycaster(_graphicRaycaster);
                Debug.Log($"[StorageEnvironment] OnDisable: {name} unregistered GraphicRaycaster {_graphicRaycaster.name}");
            }
        }

        protected virtual void SetupScrolling()
        {
            // Ensure ScrollRect on this GameObject
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>() ?? gameObject.AddComponent<ScrollRect>();

            // Ensure a content container for the scrollable area
            if (contentContainer == null)
            {
                Transform existingContent = transform.Find("Content");
                if (existingContent != null)
                {
                    contentContainer = existingContent.GetComponent<RectTransform>();
                    Debug.Log($"[StorageEnvironment] SetupScrolling: {name} found existing Content.");
                }
                else
                {
                    GameObject contentGO = new GameObject("Content", typeof(RectTransform));
                    contentContainer = contentGO.GetComponent<RectTransform>();
                    contentContainer.SetParent(transform, false);
                    contentContainer.anchorMin = Vector2.zero;
                    contentContainer.anchorMax = Vector2.one;
                    contentContainer.pivot = new Vector2(0.5f, 0.5f);
                    contentContainer.offsetMin = Vector2.zero;
                    contentContainer.offsetMax = Vector2.zero;
                    Debug.Log($"[StorageEnvironment] SetupScrolling: {name} created new Content.");
                }
            }

            // Configure ScrollRect
            scrollRect.content = contentContainer;
            scrollRect.viewport = GetComponent<RectTransform>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Use the content container as the placement area for blocks
            if (contentArea == null)
                contentArea = contentContainer;

            Debug.Log($"[StorageEnvironment] SetupScrolling: {name} contentArea={(contentArea != null ? contentArea.name : "NULL")}, scrollRect={(scrollRect != null)}");
        }

        protected virtual void SetupVisuals()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
                if (backgroundSprite != null)
                    backgroundImage.sprite = backgroundSprite;
                backgroundImage.type = Image.Type.Sliced;
            }
        }

        public override bool CanPlaceBlock(I_BE2_Block block) => true;
        public override bool CanPickupBlock(I_BE2_Block block) => true;
    }
}
