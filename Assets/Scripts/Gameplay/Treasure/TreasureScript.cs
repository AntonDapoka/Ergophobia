using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Environment;

public class TreasureScript : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("References")]
    [SerializeField] private SphereCollider colliderInteraction;
    [SerializeField] private GameObject hint;
    [SerializeField] private ChestEnvironment chestEnvironmentTemplate;
    [SerializeField] private Transform chestSpawnParent;
    [SerializeField] private BlocksReferenceScript blocksReference;
    [SerializeField] private GameObject VFX;
    private GameObject activeVFXInstance;
    private Animator animator;
    private AudioSource audioSource;
    [SerializeField] private AudioClip openSFX;
    [Header("Specific Blocks")]
    [Tooltip("If assigned, these exact prefabs are used instead of random blocks from BlocksReference." +
             " Use this for developer-placed chests in the start room.")]
    [SerializeField] private List<GameObject> specificBlockPrefabs = new();

    [Header("Settings")]
    [SerializeField] private int rewardBlockCount = 3;

    private bool isPlayerInRange;
    private bool isOpened;
    private ChestEnvironment activeChestEnvironment;
    private List<GameObject> currentRewardPrefabs = new();

    private static readonly List<TreasureScript> activeTreasures = new();
    public static IReadOnlyList<TreasureScript> ActiveTreasures => activeTreasures;

    public bool IsOpened => isOpened;
    public ChestEnvironment ActiveChestEnvironment => activeChestEnvironment;
    public IReadOnlyList<GameObject> CurrentRewardPrefabs => currentRewardPrefabs;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (chestEnvironmentTemplate == null && TreasureSpawnerScript.Instance != null)
        {
            chestEnvironmentTemplate = TreasureSpawnerScript.Instance.ChestEnvironmentTemplate;
            chestSpawnParent = TreasureSpawnerScript.Instance.ChestSpawnParent;
            if (blocksReference == null)
                blocksReference = TreasureSpawnerScript.Instance.BlocksReference;
        }
    }

    private void OnEnable()
    {
        activeTreasures.Add(this);
    }

    private void OnDisable()
    {
        if (activeTreasures.Contains(this))
            activeTreasures.Remove(this);
    }

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

    public void Open()
    {
        animator.SetTrigger("Open");
        audioSource.PlayOneShot(openSFX);
        if (VFX != null)
        activeVFXInstance = Instantiate(VFX, transform.position, Quaternion.identity, transform);
        activeVFXInstance.SetActive(true);
        if (isOpened) return;
        isOpened = true;

        if (hint != null)
            hint.SetActive(false);

        if (colliderInteraction != null)
            colliderInteraction.enabled = false;

        if (chestEnvironmentTemplate == null)
        {
            Debug.LogWarning($"[TreasureScript] '{name}' has no ChestEnvironment template assigned.", this);
            return;
        }

        if (chestEnvironmentTemplate.gameObject.activeInHierarchy)
            chestEnvironmentTemplate.gameObject.SetActive(false);

        Transform parent = chestSpawnParent != null ? chestSpawnParent : chestEnvironmentTemplate.transform.parent;
        activeChestEnvironment = Instantiate(chestEnvironmentTemplate, parent);
        activeChestEnvironment.gameObject.SetActive(true);

        currentRewardPrefabs = BuildRewardPrefabs();
        activeChestEnvironment.PopulateWithPrefabs(currentRewardPrefabs);
        activeChestEnvironment.OnBlockSelected += HandleBlockSelected;

        Debug.Log($"[TreasureScript] '{name}' opened with {currentRewardPrefabs.Count} blocks.");
    }

    public void Setup(ChestEnvironment chestTemplate, Transform spawnParent, BlocksReferenceScript blocks)
    {
        chestEnvironmentTemplate = chestTemplate;
        chestSpawnParent = spawnParent;
        blocksReference = blocks;
    }

    public void ForceDestroy()
    {
        if (activeChestEnvironment != null)
        {
            activeChestEnvironment.Clear();
            Destroy(activeChestEnvironment.gameObject);
            activeChestEnvironment = null;
        }

        Destroy(gameObject);
    }

    private List<GameObject> BuildRewardPrefabs()
    {
        if (specificBlockPrefabs != null && specificBlockPrefabs.Count > 0)
            return new List<GameObject>(specificBlockPrefabs);

        if (blocksReference == null)
        {
            Debug.LogWarning($"[TreasureScript] '{name}' has no BlocksReferenceScript and no specific blocks.", this);
            return new List<GameObject>();
        }

        GameObject[] available = blocksReference.GetBlocks();
        if (available == null || available.Length == 0)
        {
            Debug.LogWarning($"[TreasureScript] '{name}' has no available blocks.", this);
            return new List<GameObject>();
        }

        var result = new List<GameObject>();
        var pool = new List<GameObject>(available);
        int count = Mathf.Min(rewardBlockCount, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    private void HandleBlockSelected(I_BE2_Block selectedBlock)
    {
        if (activeChestEnvironment == null) return;

        activeChestEnvironment.OnBlockSelected -= HandleBlockSelected;
        activeChestEnvironment.DestroyRemainingBlocks(selectedBlock);

        StartCoroutine(DestroyChestAfterSelection());
    }

    private IEnumerator DestroyChestAfterSelection()
    {
        yield return null;

        if (activeChestEnvironment != null)
        {
            activeChestEnvironment.gameObject.SetActive(false);
            Destroy(activeChestEnvironment.gameObject);
            activeChestEnvironment = null;
        }

        if (activeVFXInstance != null)
        {
            Destroy(activeVFXInstance, 0.5f);
            activeVFXInstance = null;
        }

        Destroy(gameObject);
    }
}
