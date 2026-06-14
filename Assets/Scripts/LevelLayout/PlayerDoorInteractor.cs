using System.Collections;
using UnityEngine;

public class PlayerDoorInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactionKey = KeyCode.T;

    [Header("References")]
    [SerializeField] private LevelTransitionManager transitionManager;

    private DoorScript currentDoor;
    private bool isTransitioning;

    private void OnTriggerEnter(Collider other)
    {
        DoorMarker marker = other.GetComponent<DoorMarker>();
        if (marker == null) return;

        DoorScript door = marker.GetComponentInParent<DoorScript>();
        if (door == null) return;

        currentDoor = door;
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentDoor != null) return;

        DoorMarker marker = other.GetComponent<DoorMarker>();
        if (marker == null) return;

        DoorScript door = marker.GetComponentInParent<DoorScript>();
        if (door == null) return;

        currentDoor = door;
    }

    private void OnTriggerExit(Collider other)
    {
        DoorMarker marker = other.GetComponent<DoorMarker>();
        if (marker == null) return;

        DoorScript door = marker.GetComponentInParent<DoorScript>();
        if (door == currentDoor)
            currentDoor = null;
    }

    private void Update()
    {
        if (isTransitioning) return;
        if (currentDoor == null) return;
        if (!currentDoor.IsConnected) return;
        if (!Input.GetKeyDown(interactionKey)) return;

        DoorScript targetDoor = currentDoor.OppositeDoor;
        if (targetDoor == null) return;

        Transform entryPoint = targetDoor.PlayerEntryPoint;
        if (entryPoint == null)
        {
            Debug.LogWarning($"Door '{targetDoor.name}' has no PlayerEntryPoint");
            return;
        }

        StartCoroutine(HandleTransition(targetDoor, entryPoint.position));
    }

    private IEnumerator HandleTransition(DoorScript targetDoor, Vector3 entryPosition)
    {
        isTransitioning = true;

        if (transitionManager != null && targetDoor.ParentRoom != null)
        {
            yield return transitionManager.TransitionToRoomWithFade(
                targetDoor.ParentRoom,
                () => TeleportTo(entryPosition)
            );
        }
        else
        {
            TeleportTo(entryPosition);
        }

        currentDoor = targetDoor;
        isTransitioning = false;
    }

    private void TeleportTo(Vector3 position)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = position;
        rb.rotation = Quaternion.identity;
    }
}
