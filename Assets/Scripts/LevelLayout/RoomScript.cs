using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomScript : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Vector2 roomSize = new(10f, 10f);
    [SerializeField] private DoorScript[] doors;

    private Vector2Int gridPosition;
    public RoomSlot CurrentSlot { get; private set; }

    public Vector2Int GridPosition => gridPosition;
    public Vector2 RoomSize => roomSize;
    public RoomPrefabConfig SourceConfig { get; set; }

    public void Initialize(Vector2Int gridPos)
    {
        gridPosition = gridPos;
        CurrentSlot = new RoomSlot(gridPos.x, gridPos.y);

        if (doors == null || doors.Length == 0)
            doors = GetComponentsInChildren<DoorScript>(true);

        doors = doors.Where(d => d != null).ToArray();

        foreach (var door in doors)
            door.Initialize(this);
    }

    public void Initialize(RoomSlot slot)
    {
        CurrentSlot = slot;
        gridPosition = new Vector2Int(slot.X, slot.Lane);

        if (doors == null || doors.Length == 0)
            doors = GetComponentsInChildren<DoorScript>(true);

        doors = doors.Where(d => d != null).ToArray();

        foreach (var door in doors)
            door.Initialize(this);
    }

    public bool HasDoor(DoorType type)
    {
        return doors != null && doors.Any(d => d != null && d.DoorType == type);
    }

    public DoorScript GetDoor(DoorType type)
    {
        if (doors == null) return null;
        return doors.FirstOrDefault(d => d != null && d.DoorType == type);
    }

    public List<DoorScript> GetDoors()
    {
        if (doors == null) return new List<DoorScript>();
        return doors.Where(d => d != null).ToList();
    }

    public List<DoorScript> GetAvailableDoors()
    {
        if (doors == null) return new List<DoorScript>();
        return doors.Where(d => d != null && !d.IsConnected && !d.IsBlockedByPrefab).ToList();
    }

    public List<DoorScript> GetAvailableDoorsByDirection(DoorDirection direction)
    {
        if (doors == null) return new List<DoorScript>();
        return doors.Where(d =>
            d != null &&
            DoorTypeHelper.GetDirection(d.DoorType) == direction &&
            !d.IsConnected &&
            !d.IsBlockedByPrefab
        ).ToList();
    }

    public bool HasAvailableExit(DoorDirection direction)
    {
        return GetAvailableDoorsByDirection(direction).Count > 0;
    }

    public void SealUnusedDoors()
    {
        if (doors == null) return;
        foreach (var door in doors)
        {
            if (door != null)
                door.Seal();
        }
    }

    public void SetDoorsFromConfig(RoomPrefabConfig config)
    {
        if (doors == null || doors.Length == 0)
            doors = GetComponentsInChildren<DoorScript>(true);

        if (doors == null) return;

        foreach (var door in doors)
        {
            if (door == null) continue;
            if (config.blockedDoors.Contains(door.DoorType))
            {
                door.SetBlockedByPrefab(true);
            }
        }
    }
}
