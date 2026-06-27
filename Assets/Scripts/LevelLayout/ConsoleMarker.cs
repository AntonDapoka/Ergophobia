using UnityEngine;

/// <summary>
/// Trigger relay placed on the console object (or its child) to detect the player.
/// Mirrors the marker pattern used by <see cref="TreasureMarker"/> and <see cref="DoorMarker"/>.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ConsoleMarker : MonoBehaviour
{
    private ConsoleScript console;

    private void Awake()
    {
        console = GetComponentInParent<ConsoleScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            console?.OnPlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            console?.OnPlayerExited();
    }
}
