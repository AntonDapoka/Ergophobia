using System;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Core;

namespace MG_BlocksEngine2.Environment
{
    /// <summary>
    /// A read-only storage from which blocks can be taken but not placed back.
    /// Inherits scrolling and visuals from StorageEnvironment.
    /// Designed for manual population via the Inspector (prefab list) or
    /// runtime population from a block reference (e.g. treasure rewards).
    /// </summary>
    public class ChestEnvironment : StorageEnvironment
    {
        [Header("Spawner")]
        [Tooltip("Anchor point inside the chest where blocks will be spawned around.")]
        public Transform spawnAnchor;
        [Tooltip("Optional explicit spawn points inside the chest. If empty, blocks are spread radially around the anchor.")]
        public Transform[] spawnPoints;
        [Tooltip("Prefabs to spawn when Populate() is called manually.")]
        public List<GameObject> blockPrefabs = new List<GameObject>();
        [Tooltip("Radius for spreading spawned blocks around the anchor when no explicit spawn points are set.")]
        public float spawnSpreadRadius = 30f;
        [Tooltip("How many random blocks should be pulled from a reference when PopulateWithRandomBlocks is called.")]
        public int randomBlockCount = 3;

        /// <summary>
        /// Invoked when the player has dragged a block out and dropped it somewhere.
        /// Passes the selected block (may be null if it was destroyed mid-drop).
        /// </summary>
        public event Action<I_BE2_Block> OnBlockSelected;

        private I_BE2_Block pendingSelectedBlock;
        private HashSet<I_BE2_Block> ownedBlocks = new HashSet<I_BE2_Block>();
        private bool isClosed;
        private CanvasGroup canvasGroup;

        protected override void OnDisable()
        {
            base.OnDisable();

            BE2_MainEventsManager.Instance?.StopListening(BE2EventTypesBlock.OnDragOut, HandleDragOut);
            BE2_MainEventsManager.Instance?.StopListening(BE2EventTypesBlock.OnDrop, HandleBlockDrop);
        }

        protected override void Awake()
        {
            base.Awake();

            if (spawnAnchor == null && contentArea != null)
                spawnAnchor = contentArea;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            isClosed = false;

            BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnDragOut, HandleDragOut);
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypesBlock.OnDrop, HandleBlockDrop);
        }

        public override bool CanPlaceBlock(I_BE2_Block block) => false;
        public override bool CanPickupBlock(I_BE2_Block block) => !isClosed;

        /// <summary>
        /// Once a block has been picked, redirect any block that would be placed back
        /// into this chest to the nearest other StorageEnvironment instead.
        /// </summary>
        public override void AddBlock(I_BE2_Block block, Vector2 localPosition)
        {
            if (!isClosed)
            {
                base.AddBlock(block, localPosition);
                return;
            }

            StorageEnvironment target = FindNearestOtherStorage(block.Transform.position);
            if (target != null)
            {
                Vector2 targetLocalPos = target.contentArea.InverseTransformPoint(block.Transform.position);
                target.AddBlock(block, targetLocalPos);
            }
            else
            {
                base.AddBlock(block, localPosition);
            }
        }

        private void CloseInteraction()
        {
            if (isClosed) return;
            isClosed = true;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private StorageEnvironment FindNearestOtherStorage(Vector3 worldPoint)
        {
            StorageEnvironment nearest = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (var holder in ActiveHolders)
            {
                if (holder == this) continue;
                if (holder is not StorageEnvironment storage) continue;
                if (storage.contentArea == null) continue;
                if (!storage.CanPlaceBlock(null)) continue;

                Vector2 nearestPoint = GetNearestPointOnRect(storage.contentArea, worldPoint);
                float distanceSqr = ((Vector2)nearestPoint - (Vector2)worldPoint).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearest = storage;
                }
            }

            return nearest;
        }

        private Vector2 GetNearestPointOnRect(RectTransform rectTransform, Vector3 worldPoint)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            return new Vector2(
                Mathf.Clamp(worldPoint.x, minX, maxX),
                Mathf.Clamp(worldPoint.y, minY, maxY));
        }

        /// <summary>
        /// Manually populate the chest with the configured block prefabs.
        /// Call this from editor scripts, buttons, or future database loaders.
        /// </summary>
        public void Populate()
        {
            if (contentArea == null) return;

            Clear();

            for (int i = 0; i < blockPrefabs.Count; i++)
            {
                if (blockPrefabs[i] == null) continue;
                SpawnBlockAtSlot(blockPrefabs[i], i, blockPrefabs.Count);
            }
        }

        /// <summary>
        /// Populate the chest with exact prefabs. Used by TreasureScript to fill
        /// a per-treasure chest instance.
        /// </summary>
        public void PopulateWithPrefabs(List<GameObject> prefabs)
        {
            if (contentArea == null) return;

            Clear();

            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] == null) continue;
                SpawnBlockAtSlot(prefabs[i], i, prefabs.Count);
            }
        }

        /// <summary>
        /// Populate the chest with <paramref name="count"/> random blocks chosen from
        /// <paramref name="availableBlocks"/>. Blocks spawn at the configured spawn points
        /// (or radially around the anchor if no spawn points are set).
        /// </summary>
        public void PopulateWithRandomBlocks(GameObject[] availableBlocks, int count)
        {
            if (contentArea == null) return;
            if (availableBlocks == null || availableBlocks.Length == 0)
            {
                Debug.LogWarning($"[ChestEnvironment] '{name}' received no available blocks to populate.", this);
                return;
            }

            Clear();

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = PickRandomBlock(availableBlocks);
                if (prefab == null) continue;
                SpawnBlockAtSlot(prefab, i, count);
            }
        }

        /// <summary>
        /// Convenience overload that uses the inspector-configured <see cref="randomBlockCount"/>.
        /// </summary>
        public void PopulateWithRandomBlocks(GameObject[] availableBlocks)
        {
            PopulateWithRandomBlocks(availableBlocks, randomBlockCount);
        }

        /// <summary>
        /// Remove and destroy all blocks currently in the chest.
        /// </summary>
        public void Clear()
        {
            for (int i = Blocks.Count - 1; i >= 0; i--)
            {
                if (Blocks[i] != null && Blocks[i].Transform != null)
                    Destroy(Blocks[i].Transform.gameObject);
            }
            Blocks.Clear();
            ownedBlocks.Clear();
        }

        /// <summary>
        /// Destroys every block still inside the chest except the selected one.
        /// </summary>
        public void DestroyRemainingBlocks(I_BE2_Block exceptBlock)
        {
            for (int i = Blocks.Count - 1; i >= 0; i--)
            {
                I_BE2_Block block = Blocks[i];
                if (block != exceptBlock && block != null && block.Transform != null)
                    Destroy(block.Transform.gameObject);
            }

            Blocks.Clear();
            if (exceptBlock != null)
                Blocks.Add(exceptBlock);
        }

        private void HandleDragOut(I_BE2_Block draggedBlock)
        {
            if (pendingSelectedBlock != null) return;
            if (draggedBlock == null) return;
            if (!ownedBlocks.Contains(draggedBlock)) return;

            pendingSelectedBlock = draggedBlock;
            CloseInteraction();
        }

        private void HandleBlockDrop(I_BE2_Block droppedBlock)
        {
            if (pendingSelectedBlock == null) return;

            // Finalize when the pending block is dropped or destroyed mid-drop.
            if (droppedBlock == null || droppedBlock == pendingSelectedBlock)
                FinalizeSelection();
        }

        private void FinalizeSelection()
        {
            if (pendingSelectedBlock == null) return;

            I_BE2_Block selected = pendingSelectedBlock;
            pendingSelectedBlock = null;
            OnBlockSelected?.Invoke(selected);
        }

        private GameObject PickRandomBlock(GameObject[] availableBlocks)
        {
            int index = UnityEngine.Random.Range(0, availableBlocks.Length);
            return availableBlocks[index];
        }

        private void SpawnBlockAtSlot(GameObject prefab, int slotIndex, int totalSlots)
        {
            GameObject blockGO = Instantiate(prefab, contentArea);
            blockGO.name = prefab.name;

            if (spawnPoints != null && slotIndex < spawnPoints.Length && spawnPoints[slotIndex] != null)
            {
                // Use an explicit spawn point
                blockGO.transform.localPosition = spawnPoints[slotIndex].localPosition;
                blockGO.transform.localScale = spawnPoints[slotIndex].localScale;
                blockGO.transform.localEulerAngles = spawnPoints[slotIndex].localEulerAngles;
            }
            else
            {
                // Fall back to radial spread around the anchor
                if (spawnAnchor == null) return;

                float angle = (slotIndex / (float)Mathf.Max(1, totalSlots)) * Mathf.PI * 2f;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnSpreadRadius;
                blockGO.transform.localPosition = (Vector2)spawnAnchor.localPosition + offset;
                blockGO.transform.localScale = Vector3.one;
                blockGO.transform.localEulerAngles = Vector3.zero;
            }

            I_BE2_Block block = blockGO.GetComponent<I_BE2_Block>();
            if (block != null)
            {
                Blocks.Add(block);
                ownedBlocks.Add(block);
            }
        }
    }
}
