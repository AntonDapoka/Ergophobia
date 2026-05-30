using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private DoorType doorType;
    [SerializeField] private bool isBlockedByPrefab;

    [Header("Runtime")]
    public bool IsConnected { get; private set; }
    public bool IsOpen { get; private set; }
    public RoomScript ParentRoom { get; private set; }
    public DoorScript ConnectedDoor { get; private set; }
    public RoomScript ConnectedRoom => ConnectedDoor?.ParentRoom;

    public DoorType DoorType => doorType;
    public bool IsBlockedByPrefab => isBlockedByPrefab;

    public void Initialize(RoomScript parent)
    {
        ParentRoom = parent;
        IsConnected = false;
        IsOpen = false;
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
    }

    public void Seal()
    {
        if (IsConnected || isBlockedByPrefab) return;
        gameObject.SetActive(false);
    }
}
