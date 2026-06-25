using UnityEngine;

public class TreasureMarker : MonoBehaviour
{
    private TreasureScript treasure;

    private void Awake()
    {
        treasure = GetComponentInParent<TreasureScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            treasure?.OnPlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerMarker>(out _))
            treasure?.OnPlayerExited();
    }
}
