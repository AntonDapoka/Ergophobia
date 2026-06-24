using UnityEngine;

public class DoorMarker : MonoBehaviour
{
    private DoorScript door;

    private void Awake()
    {
        door = GetComponentInParent<DoorScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            door?.OnPlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            door?.OnPlayerExited();
    }
}
