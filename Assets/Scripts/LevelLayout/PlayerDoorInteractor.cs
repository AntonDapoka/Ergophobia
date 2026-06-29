using System.Collections;
using UnityEngine;

public class PlayerDoorInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactionKey = KeyCode.T;

    [Header("References")]
    [SerializeField] private LevelTransitionManager transitionManager;

    private DoorScript currentDoor;
    private Collider currentCollider;
    private bool isTransitioning;

    private void OnTriggerEnter(Collider other)
    {
        TrySetCurrentDoor(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Always refresh the current door while overlapping a marker.
        // This prevents stale references if the player teleports or a
        // collider is disabled/enabled.
        TrySetCurrentDoor(other);
    }

    private void OnTriggerExit(Collider other)
    {
        DoorMarker marker = other.GetComponent<DoorMarker>();
        if (marker == null) return;

        DoorScript door = marker.GetComponentInParent<DoorScript>();
        if (door == null) return;

        if (door == currentDoor)
            ClearCurrentDoor();
    }

    private void Update()
    {
        if (isTransitioning) return;

        ValidateCurrentDoor();

        if (currentDoor == null) return;
        if (!currentDoor.IsConnected) return;
        if (!Input.GetKeyDown(interactionKey)) return;

        DoorScript targetDoor = currentDoor.OppositeDoor;
        if (targetDoor == null) return;

        if (transitionManager != null && !transitionManager.CanLeaveCurrentRoom())
        {
            Debug.Log("Cannot leave -enemies remain.");
            return;
        }

        Transform entryPoint = targetDoor.PlayerEntryPoint;
        if (entryPoint == null)
        {
            Debug.LogWarning($"Door '{targetDoor.name}' has no PlayerEntryPoint");
            return;
        }

        StartCoroutine(HandleTransition(targetDoor, entryPoint.position));
    }

    private void TrySetCurrentDoor(Collider other)
    {
        DoorMarker marker = other.GetComponent<DoorMarker>();
        if (marker == null) return;

        DoorScript door = marker.GetComponentInParent<DoorScript>();
        if (door == null) return;

        currentDoor = door;
        currentCollider = other;
    }

    private void ValidateCurrentDoor()
    {
        if (currentDoor == null) return;

        if (currentCollider == null || !currentCollider.gameObject.activeInHierarchy)
        {
            ClearCurrentDoor();
            return;
        }

        Vector3 closestPoint = currentCollider.ClosestPoint(transform.position);
        if (Vector3.Distance(closestPoint, transform.position) > 0.1f)
        {
            ClearCurrentDoor();
        }
    }

    private void ClearCurrentDoor()
    {
        currentDoor = null;
        currentCollider = null;
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

        ClearCurrentDoor();
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
