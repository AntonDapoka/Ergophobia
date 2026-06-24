using UnityEngine;

[RequireComponent(typeof(DoorReference))]
public class DoorScript : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private DoorType doorType;
    [SerializeField] private bool isBlockedByPrefab;

    [Header("Config")]
    [SerializeField] private CapsuleCollider colliderDoor;
    private DoorReference reference;

    [Header("Runtime")]
    public bool IsConnected { get; private set; }
    public bool IsOpen { get; private set; }
    public RoomScript ParentRoom { get; private set; }
    public DoorScript ConnectedDoor { get; private set; }
    public DoorScript OppositeDoor => ConnectedDoor;
    public RoomScript ConnectedRoom => ConnectedDoor?.ParentRoom;

    public DoorType DoorType => doorType;
    public bool IsBlockedByPrefab => isBlockedByPrefab;
    public CapsuleCollider ColliderDoor => colliderDoor;

    public Transform PlayerEntryPoint => reference?.GetPositionPlayerEntry();

    public void Initialize(RoomScript parent)
    {
        ParentRoom = parent;
        IsConnected = false;
        IsOpen = false;
        reference = GetComponent<DoorReference>();
    }

    public void SetBlockedByPrefab(bool blocked)
    {
        isBlockedByPrefab = blocked;
    }

    public void ConnectTo(DoorScript other)
    {
        if (other == null) return;
        ConnectedDoor = other;
        IsConnected = true;
        IsOpen = true;

        reference?.SetConnectedDoor(other);
        other.reference?.SetConnectedDoor(this);
    }

    public void Seal()
    {
        if (IsConnected || isBlockedByPrefab) return;
        gameObject.SetActive(false);
    }

    public void SetCamera(Transform camera)
    {
        reference?.InitializeHint(camera);
    }

    public void OnPlayerEntered()
    {
        if (!IsConnected) return;
        reference?.SetHintActive(true);
    }

    public void OnPlayerExited()
    {
        reference?.SetHintActive(false);
    }
}
