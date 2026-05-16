using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block;

namespace MG_BlocksEngine2.Environment
{
    public abstract class HolderEnvironment : MonoBehaviour
    {
        [Header("Content Area")]
        public RectTransform contentArea;

        [Header("Visuals")]
        public Image backgroundImage;
        public Color hoverTint = new Color(1f, 1f, 1f, 0.1f);
        Color _defaultBackgroundColor;

        public List<I_BE2_Block> Blocks { get; private set; } = new List<I_BE2_Block>();

        static List<HolderEnvironment> _activeHolders = new List<HolderEnvironment>();
        public static IReadOnlyList<HolderEnvironment> ActiveHolders => _activeHolders;

        void OnEnable()
        {
            if (!_activeHolders.Contains(this))
                _activeHolders.Add(this);

            if (contentArea == null)
                contentArea = GetComponent<RectTransform>();

            if (backgroundImage != null)
                _defaultBackgroundColor = backgroundImage.color;
        }

        void OnDisable()
        {
            _activeHolders.Remove(this);
        }

        public virtual bool CanPlaceBlock(I_BE2_Block block) => true;
        public virtual bool CanPickupBlock(I_BE2_Block block) => true;

        public void AddBlock(I_BE2_Block block, Vector2 localPosition)
        {
            if (block == null) return;

            block.Transform.SetParent(contentArea, false);
            block.Transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0);
            block.Transform.localScale = Vector3.one;
            block.Transform.localEulerAngles = Vector3.zero;

            if (!Blocks.Contains(block))
                Blocks.Add(block);
        }

        public void RemoveBlock(I_BE2_Block block)
        {
            if (block == null) return;
            Blocks.Remove(block);
        }

        public bool ContainsPoint(Vector2 worldPoint)
        {
            if (contentArea == null) return false;

            Vector3[] corners = new Vector3[4];
            contentArea.GetWorldCorners(corners);

            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            return worldPoint.x >= minX && worldPoint.x <= maxX && worldPoint.y >= minY && worldPoint.y <= maxY;
        }

        public void SetHoverVisual(bool isHovered)
        {
            if (backgroundImage == null) return;
            backgroundImage.color = isHovered
                ? _defaultBackgroundColor + hoverTint
                : _defaultBackgroundColor;
        }
    }
}
