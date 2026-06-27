using UnityEngine;

public class ConsoleManagerScript : MonoBehaviour
{
    public static ConsoleManagerScript Instance { get; private set; }

    [Header("Systems")]
    [SerializeField] private StageSwitchScript stageSwitch;
    [SerializeField] private WinScript winScript;

    [Header("Settings")]
    [SerializeField] private int interactionsToWin = 3;

    public int InteractionCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ConsoleManagerScript] Duplicate found. Destroying this one.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void OnConsoleInteracted()
    {
        InteractionCount++;

        // Final interaction: show win screen, do NOT switch stage / teleport.
        if (InteractionCount >= interactionsToWin)
        {
            if (winScript != null)
                winScript.Win();
            else
                Debug.LogWarning("[ConsoleManagerScript] WinScript is not assigned.", this);

            return;
        }

        if (stageSwitch != null)
            stageSwitch.SwitchStage();
        else
            Debug.LogWarning("[ConsoleManagerScript] StageSwitchScript is not assigned.", this);
    }
}
