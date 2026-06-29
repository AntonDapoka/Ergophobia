
using UnityEngine;
using UnityEngine.UI;
using MG_BlocksEngine2.DragDrop;
using MG_BlocksEngine2.Environment;

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

        public int NestingLevel
        {
            get
            {
                if (ParentBody == null)
                    return 0;

                if (ParentBody.BlockSection != null && ParentBody.BlockSection.Block != null)
                {
                    BE2_Line parentLine = ParentBody.BlockSection.Block.Transform.GetComponentInParent<BE2_Line>();
                    if (parentLine != null)
                        return parentLine.NestingLevel + 1;
                }

                return 1;
            }
        }

        public Color NormalColor = new(0.15f, 0.15f, 0.15f, 0.3f);
        public Color HoverColor = new(0.3f, 0.5f, 0.8f, 0.5f);
        public Color OccupiedColor = new(0.1f, 0.1f, 0.1f, 0.1f);
        public Color ActiveColor = new(0.6f, 0.0f, 0.08f, 0.6f);
        public Sprite BackgroundSprite;

        bool _isHighlighted;

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
                // Preserve the block's intended size before parenting
                Vector2 intendedSize = block.Layout != null ? block.Layout.Size : (block.Transform as RectTransform).sizeDelta;

                block.Transform.SetParent(transform, false);
                block.Transform.localPosition = Vector3.zero;
                block.Transform.localScale = Vector3.one;
                block.Transform.localEulerAngles = Vector3.zero;

                // Force immediate layout update so the block keeps its correct size
                if (block.Layout != null)
                {
                    block.Layout.UpdateLayout();
                }

                // Defensive: if layout collapsed the size, restore the intended size
                RectTransform blockRT = block.Transform as RectTransform;
                if (blockRT != null && blockRT.sizeDelta.x < intendedSize.x * 0.5f)
                {
                    blockRT.sizeDelta = intendedSize;
                }

                var env = GetComponentInParent<BE2_ProgrammingEnv>();
                if (env != null && block.Instruction?.InstructionBase != null)
                    block.Instruction.InstructionBase.TargetObject = env.TargetObject;
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
            if (_image == null) return;
            if (_isHighlighted)
                _image.color = ActiveColor;
            else
                _image.color = isHovered ? HoverColor : (IsOccupied ? OccupiedColor : NormalColor);
        }

        public void SetActiveHighlight(bool active)
        {
            _isHighlighted = active;
            UpdateVisual();
        }

        void UpdateVisual()
        {
            if (_image == null) return;
            if (_isHighlighted)
                _image.color = ActiveColor;
            else
                _image.color = IsOccupied ? OccupiedColor : NormalColor;
        }
    }
}
