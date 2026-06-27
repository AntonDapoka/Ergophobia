using UnityEngine;

using MG_BlocksEngine2.Environment;

public class TreasureScript : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("References")]
    [SerializeField] private SphereCollider colliderInteraction;
    [SerializeField] private GameObject hint;
    [SerializeField] private ChestEnvironment chestEnvironment;
    [SerializeField] private BlocksReferenceScript blocksReference;

    [Header("Settings")]
    [SerializeField] private int rewardBlockCount = 3;

    private bool isPlayerInRange;
    private bool isOpened;

    public bool IsOpened => isOpened;

    public void OnPlayerEntered()
    {
        isPlayerInRange = true;
        if (hint != null && !isOpened)
            hint.SetActive(true);
    }

    public void OnPlayerExited()
    {
        isPlayerInRange = false;
        if (hint != null)
            hint.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange || isOpened) return;
        if (!Input.GetKeyDown(interactionKey)) return;

        Open();
    }

    /// <summary>
    /// Opens the treasure: hides the interaction hint, disables repeated opening,
    /// activates the chest environment and fills it with random reward blocks.
    /// </summary>
    public void Open()
    {
        if (isOpened) return;
        isOpened = true;

        if (hint != null)
            hint.SetActive(false);

        if (colliderInteraction != null)
            colliderInteraction.enabled = false;

        if (chestEnvironment == null)
        {
            Debug.LogWarning($"[TreasureScript] '{name}' has no ChestEnvironment assigned.", this);
            return;
        }

        if (blocksReference == null)
        {
            Debug.LogWarning($"[TreasureScript] '{name}' has no BlocksReferenceScript assigned.", this);
            return;
        }

        chestEnvironment.gameObject.SetActive(true);
        chestEnvironment.PopulateWithRandomBlocks(blocksReference.GetBlocks(), rewardBlockCount);

        Debug.Log($"[TreasureScript] '{name}' opened and granted {rewardBlockCount} blocks.");
    }

    /// <summary>
    /// Used by spawners to wire up the scene references after instantiation.
    /// </summary>
    public void Setup(ChestEnvironment chest, BlocksReferenceScript blocks)
    {
        chestEnvironment = chest;
        blocksReference = blocks;
    }
}
