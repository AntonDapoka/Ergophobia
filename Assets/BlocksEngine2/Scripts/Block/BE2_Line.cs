
using UnityEngine;
using UnityEngine.UI;
using MG_BlocksEngine2.DragDrop;

namespace MG_BlocksEngine2.Block
{
    public class BE2_Line : MonoBehaviour, I_BE2_Spot
    {
        BE2_DragDropManager _dragDropManager;
        RectTransform _rectTransform;
        Image _image;
        Transform _transform;

        public Transform Transform => _transform ? _transform : transform;
        public Vector2 DropPosition
        {
            get
            {
                Vector3[] corners = new Vector3[4];
                _rectTransform.GetWorldCorners(corners);
                return (corners[0] + corners[2]) * 0.5f;
            }
        }
        public I_BE2_Block Block => transform.GetComponentInParent<I_BE2_Block>();

        [SerializeField]
        int _lineIndex;
        public int LineIndex { get => _lineIndex; set => _lineIndex = value; }

        public I_BE2_Block CurrentBlock { get; private set; }
        public BE2_BlockSectionBody ParentBody { get; set; }
        public bool IsOccupied => CurrentBlock != null;

        public Color NormalColor = new Color(0.15f, 0.15f, 0.15f, 0.3f);
        public Color HoverColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        public Color OccupiedColor = new Color(0.1f, 0.1f, 0.1f, 0.1f);
        public Sprite BackgroundSprite;

        void Awake()
        {
            _transform = transform;
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
        }

        void OnEnable()
        {
            _dragDropManager = BE2_DragDropManager.Instance;
            _dragDropManager?.AddToSpotsList(this);
            UpdateVisual();
        }

        void OnDisable()
        {
            _dragDropManager?.RemoveFromSpotsList(this);
        }

        public void SetBlock(I_BE2_Block block)
        {
            if (CurrentBlock != null && block != null)
                return;

            CurrentBlock = block;
            if (block != null)
            {
                block.Transform.SetParent(transform, false);
                block.Transform.localPosition = Vector3.zero;
                block.Transform.localScale = Vector3.one;
                block.Transform.localEulerAngles = Vector3.zero;
            }
            UpdateVisual();
        }

        public void ClearBlock()
        {
            CurrentBlock = null;
            UpdateVisual();
        }

        public void SetHover(bool isHovered)
        {
            if (_image != null)
                _image.color = isHovered ? HoverColor : (IsOccupied ? OccupiedColor : NormalColor);
        }

        void UpdateVisual()
        {
            if (_image != null)
                _image.color = IsOccupied ? OccupiedColor : NormalColor;
        }
    }
}
