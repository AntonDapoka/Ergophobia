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
            // Remove destroyed holders that may linger in the static list after scene reloads
            for (int i = _activeHolders.Count - 1; i >= 0; i--)
            {
                if (_activeHolders[i] == null)
                {
                    _activeHolders.RemoveAt(i);
                    Debug.Log($"[HolderEnvironment] OnEnable: cleaned up destroyed holder at index {i}.");
                }
            }

            if (!_activeHolders.Contains(this))
            {
                _activeHolders.Add(this);
                Debug.Log($"[HolderEnvironment] OnEnable: {name} registered. Total active holders={_activeHolders.Count}");
            }

            if (contentArea == null)
                contentArea = GetComponent<RectTransform>();

            if (backgroundImage != null)
                _defaultBackgroundColor = backgroundImage.color;
        }

        void OnDisable()
        {
            if (_activeHolders.Remove(this))
                Debug.Log($"[HolderEnvironment] OnDisable: {name} unregistered. Total active holders={_activeHolders.Count}");
        }

        public virtual bool CanPlaceBlock(I_BE2_Block block)
        {
            bool result = true;
            Debug.Log($"[HolderEnvironment] CanPlaceBlock: {name} -> {result} for block={(block != null ? block.Transform.name : "NULL")}");
            return result;
        }
        public virtual bool CanPickupBlock(I_BE2_Block block)
        {
            bool result = true;
            Debug.Log($"[HolderEnvironment] CanPickupBlock: {name} -> {result} for block={(block != null ? block.Transform.name : "NULL")}");
            return result;
        }

        public void AddBlock(I_BE2_Block block, Vector2 localPosition)
        {
            if (block == null)
            {
                Debug.LogWarning("[HolderEnvironment] AddBlock: block is NULL!");
                return;
            }

            block.Transform.SetParent(contentArea, false);
            block.Transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0);
            block.Transform.localEulerAngles = Vector3.zero;

            if (!Blocks.Contains(block))
                Blocks.Add(block);

            Debug.Log($"[HolderEnvironment] AddBlock: {name} now has {Blocks.Count} blocks. Added {(block != null ? block.Transform.name : "NULL")} at localPos={localPosition}");
        }

        public void RemoveBlock(I_BE2_Block block)
        {
            if (block == null) return;
            bool removed = Blocks.Remove(block);
            Debug.Log($"[HolderEnvironment] RemoveBlock: {name} removed={(removed)} from {Blocks.Count} remaining blocks. block={(block != null ? block.Transform.name : "NULL")}");
        }

        public bool ContainsPoint(Vector2 worldPoint)
        {
            if (contentArea == null)
            {
                Debug.LogWarning($"[HolderEnvironment] ContainsPoint: {name} contentArea is NULL!");
                return false;
            }

            Vector3[] corners = new Vector3[4];
            contentArea.GetWorldCorners(corners);

            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            bool result = worldPoint.x >= minX && worldPoint.x <= maxX && worldPoint.y >= minY && worldPoint.y <= maxY;
            // Debug spam reduced — only log on true to avoid console flood
            if (result)
                Debug.Log($"[HolderEnvironment] ContainsPoint: {name} CONTAINS point {worldPoint}");
            return result;
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
