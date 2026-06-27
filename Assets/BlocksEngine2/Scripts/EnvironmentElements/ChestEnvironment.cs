using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block;

namespace MG_BlocksEngine2.Environment
{
    /// <summary>
    /// A read-only storage from which blocks can be taken but not placed back.
    /// Inherits scrolling and visuals from StorageEnvironment.
    /// Designed for manual population via the Inspector (prefab list) or
    /// runtime population from a block reference (e.g., treasure rewards).
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

        public override bool CanPlaceBlock(I_BE2_Block block) => false;
        public override bool CanPickupBlock(I_BE2_Block block) => true;

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
        }

        private GameObject PickRandomBlock(GameObject[] availableBlocks)
        {
            int index = Random.Range(0, availableBlocks.Length);
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
                Blocks.Add(block);
        }
    }
}
