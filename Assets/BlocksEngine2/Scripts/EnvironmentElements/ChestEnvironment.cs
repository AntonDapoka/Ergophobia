using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block;

namespace MG_BlocksEngine2.Environment
{
    /// <summary>
    /// A read-only storage from which blocks can be taken but not placed back.
    /// Inherits scrolling and visuals from StorageEnvironment.
    /// Designed for manual population via the Inspector (prefab list).
    /// Future extension: populate from a database with prefab references.
    /// </summary>
    public class ChestEnvironment : StorageEnvironment
    {
        [Header("Spawner")]
        [Tooltip("Anchor point inside the chest where blocks will be spawned around.")]
        public Transform spawnAnchor;
        [Tooltip("Prefabs to spawn when Populate() is called manually.")]
        public List<GameObject> blockPrefabs = new List<GameObject>();
        [Tooltip("Radius for spreading spawned blocks around the anchor.")]
        public float spawnSpreadRadius = 30f;

        public override bool CanPlaceBlock(I_BE2_Block block) => false;
        public override bool CanPickupBlock(I_BE2_Block block) => true;

        /// <summary>
        /// Manually populate the chest with the configured block prefabs.
        /// Call this from editor scripts, buttons, or future database loaders.
        /// </summary>
        public void Populate()
        {
            if (spawnAnchor == null) return;
            if (contentArea == null) return;

            Clear();

            for (int i = 0; i < blockPrefabs.Count; i++)
            {
                if (blockPrefabs[i] == null) continue;

                GameObject blockGO = Instantiate(blockPrefabs[i], contentArea);
                blockGO.name = blockPrefabs[i].name;

                // Spread blocks around the anchor in a simple radial pattern
                float angle = (i / (float)Mathf.Max(1, blockPrefabs.Count)) * Mathf.PI * 2f;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnSpreadRadius;
                blockGO.transform.localPosition = (Vector2)spawnAnchor.localPosition + offset;
                blockGO.transform.localScale = Vector3.one;
                blockGO.transform.localEulerAngles = Vector3.zero;

                I_BE2_Block block = blockGO.GetComponent<I_BE2_Block>();
                if (block != null)
                    Blocks.Add(block);
            }
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
    }
}
