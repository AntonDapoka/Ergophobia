using UnityEngine;

public class ConsoleScript : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("References")]
    [SerializeField] private SphereCollider colliderInteraction;
    [SerializeField] private GameObject hint;
    [SerializeField] private ConsoleManagerScript consoleManager;

    private bool isPlayerInRange;
    private bool isActivated;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        if (consoleManager == null)
            consoleManager = ConsoleManagerScript.Instance;
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            OnPlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            OnPlayerExited();
    }

    public void OnPlayerEntered()
    {
        isPlayerInRange = true;
        if (hint != null && !isActivated) hint.SetActive(true);
    }

    public void OnPlayerExited()
    {
        isPlayerInRange = false;
        if (hint != null) hint.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange || isActivated) return;
        if (!Input.GetKeyDown(interactionKey)) return;

        Activate();
    }

    private void Activate()
    {
        isActivated = true;

        if (hint != null)
            hint.SetActive(false);

        if (colliderInteraction != null)
            colliderInteraction.enabled = false;

        consoleManager?.OnConsoleInteracted();
    }

    private void ResetState()
    {
        isActivated = false;
        isPlayerInRange = false;

        if (hint != null)
            hint.SetActive(false);

        if (colliderInteraction != null)
            colliderInteraction.enabled = true;
    }
}
