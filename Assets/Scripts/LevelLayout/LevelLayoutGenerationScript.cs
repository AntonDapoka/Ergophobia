using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLayoutGenerationScript : MonoBehaviour
{
    private const float RoomWidth = 16f;
    private const float RoomHeight = 9f;
    private const float StaggeredOffset = RoomWidth * 0.5f;

    [Header("Prefabs")]
    [SerializeField] private List<RoomPrefabConfig> roomConfigs;
    [SerializeField] private RoomPrefabConfig startRoomConfig;

    [Header("Branch Types")]
    [SerializeField] private bool allowAlignedBranches = true;
    [SerializeField] private bool allowStaggeredBranches = true;

    [Header("Spacing")]
    [SerializeField] private float roomGapX = 0f;
    [SerializeField] private float roomGapZ = 0f;

    [Header("Generation")]
    [SerializeField] private int roomCount = 8;
    [SerializeField, Range(0f, 1f)] private float branchChance = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool generateOnStart = true;

    private List<RoomScript> spawnedRooms = new List<RoomScript>();
    private Dictionary<RoomSlot, RoomScript> slotMap = new Dictionary<RoomSlot, RoomScript>();

    private void Start()
    {
        if (generateOnStart)
            GenerateLevel();
    }

    public void GenerateLevel()
    {
        ClearLevel();

        if (roomConfigs == null || roomConfigs.Count == 0)
        {
            Debug.LogError("No room configs assigned!");
            return;
        }

        RoomScript startRoom = SpawnStartRoom();
        if (startRoom == null)
        {
            Debug.LogError("Failed to spawn start room. Check room configs.");
            return;
        }

        spawnedRooms.Add(startRoom);
        slotMap[startRoom.CurrentSlot] = startRoom;

        RoomScript previousMainRoom = startRoom;

        for (int i = 1; i < roomCount; i++)
        {
            bool isLast = i == roomCount - 1;
            RoomSlot mainSlot = new RoomSlot(i, 0);

            RoomPrefabConfig config = PickMainRoomConfig(previousMainRoom, isLast);
            if (config == null)
            {
                Debug.LogError($"No suitable room config found for main room at index {i}");
                break;
            }

            RoomScript newRoom = SpawnRoomInSlot(config, mainSlot);
            if (newRoom == null) break;

            spawnedRooms.Add(newRoom);
            slotMap[mainSlot] = newRoom;

            ConnectSlots(previousMainRoom.CurrentSlot, mainSlot, DoorDirection.East);

            if (allowAlignedBranches)
            {
                TrySpawnBranch(newRoom, DoorDirection.North, false);
                TrySpawnBranch(newRoom, DoorDirection.South, false);
            }

            if (allowStaggeredBranches)
            {
                TrySpawnBranch(newRoom, DoorDirection.North, true);
                TrySpawnBranch(newRoom, DoorDirection.South, true);
            }

            previousMainRoom = newRoom;
        }

        foreach (var room in spawnedRooms)
        {
            if (room != null)
                room.SealUnusedDoors();
        }
    }

    private RoomScript SpawnStartRoom()
    {
        RoomPrefabConfig config = startRoomConfig;

        if (config != null && config.prefab != null)
        {
            if (!config.HasDoor(DoorType.East) || config.HasDoor(DoorType.West))
            {
                Debug.LogWarning("Start room config must have East and no West door. Picking random suitable config instead.");
                config = null;
            }
        }

        if (config == null)
        {
            config = GetRandomConfig(
                new List<DoorType> { DoorType.East },
                new List<DoorType> { DoorType.West }
            );
        }

        if (config == null || config.prefab == null) return null;

        return SpawnRoomInSlot(config, new RoomSlot(0, 0));
    }

    private RoomPrefabConfig PickMainRoomConfig(RoomScript previousRoom, bool isLast)
    {
        if (!previousRoom.HasAvailableExit(DoorDirection.East))
        {
            Debug.LogError("Previous main room has no available East exit!");
            return null;
        }

        List<DoorType> required = new List<DoorType> { DoorType.West };

        if (!isLast)
        {
            required.Add(DoorType.East);
        }

        RoomPrefabConfig config = GetRandomConfig(required, new List<DoorType>());

        if (config == null && !isLast)
        {
            Debug.LogWarning("No room with both West and East found. Falling back to any room with West.");
            config = GetRandomConfig(new List<DoorType> { DoorType.West }, new List<DoorType>());
        }

        return config;
    }

    private RoomScript SpawnRoomInSlot(RoomPrefabConfig config, RoomSlot slot)
    {
        if (config == null || config.prefab == null) return null;

        Vector3 worldPos = GetSlotPosition(slot);
        GameObject go = Instantiate(config.prefab, worldPos, Quaternion.identity, transform);

        RoomScript room = go.GetComponent<RoomScript>();
        if (room == null)
        {
            Debug.LogError($"Prefab '{config.prefab.name}' is missing a RoomScript component.");
            Destroy(go);
            return null;
        }

        room.Initialize(slot);
        room.SetDoorsFromConfig(config);
        return room;
    }

    private Vector3 GetSlotPosition(RoomSlot slot)
    {
        float stepX = RoomWidth + roomGapX;
        float stepZ = RoomHeight + roomGapZ;
        float offsetX = (slot.Lane != 0 && slot.IsStaggered) ? stepX * 0.5f : 0f;
        return new Vector3(slot.X * stepX + offsetX, 0f, slot.Lane * stepZ);
    }

    private void TrySpawnBranch(RoomScript parentRoom, DoorDirection direction, bool staggered)
    {
        if (Random.value > branchChance) return;
        if (!parentRoom.HasAvailableExit(direction)) return;

        int lane = direction == DoorDirection.North ? 1 : -1;
        RoomSlot branchSlot = new RoomSlot(parentRoom.CurrentSlot.X, lane, staggered);

        if (IsBranchSlotBlocked(branchSlot)) return;

        DoorDirection opposite = DoorTypeHelper.GetOppositeDirection(direction);
        DoorType[] requiredTypes = DoorTypeHelper.GetTypesByDirection(opposite).ToArray();

        List<RoomPrefabConfig> candidates = roomConfigs.Where(c =>
            requiredTypes.Any(t => c.HasDoor(t))
        ).ToList();

        if (candidates.Count == 0) return;

        RoomPrefabConfig config = candidates[Random.Range(0, candidates.Count)];

        RoomScript branchRoom = SpawnRoomInSlot(config, branchSlot);
        if (branchRoom == null) return;

        spawnedRooms.Add(branchRoom);
        slotMap[branchSlot] = branchRoom;

        ConnectSlots(parentRoom.CurrentSlot, branchSlot, direction);
    }

    private bool IsBranchSlotBlocked(RoomSlot slot)
    {
        if (slotMap.ContainsKey(slot)) return true;

        float newX = GetSlotPosition(slot).x;

        float stepX = RoomWidth + roomGapX;
        foreach (var kvp in slotMap)
        {
            RoomSlot existing = kvp.Key;
            if (existing.Lane != slot.Lane) continue;

            float existingX = GetSlotPosition(existing).x;
            if (Mathf.Abs(existingX - newX) < stepX - 0.01f)
                return true;
        }

        return false;
    }

    private void ConnectSlots(RoomSlot fromSlot, RoomSlot toSlot, DoorDirection direction)
    {
        if (!slotMap.ContainsKey(fromSlot) || !slotMap.ContainsKey(toSlot)) return;

        RoomScript fromRoom = slotMap[fromSlot];
        RoomScript toRoom = slotMap[toSlot];
        DoorDirection opposite = DoorTypeHelper.GetOppositeDirection(direction);

        List<DoorScript> fromDoors = fromRoom.GetAvailableDoorsByDirection(direction);
        List<DoorScript> toDoors = toRoom.GetAvailableDoorsByDirection(opposite);

        if (fromDoors.Count == 0 || toDoors.Count == 0)
        {
            Debug.LogWarning($"Cannot connect slots {fromSlot} -> {toSlot}: no available doors for {direction}/{opposite}");
            return;
        }

        DoorScript fromDoor = fromDoors[Random.Range(0, fromDoors.Count)];
        DoorScript toDoor = toDoors[Random.Range(0, toDoors.Count)];

        fromDoor.ConnectTo(toDoor);
        toDoor.ConnectTo(fromDoor);
    }

    private RoomPrefabConfig GetRandomConfig(List<DoorType> requiredDoors, List<DoorType> forbiddenDoors)
    {
        List<RoomPrefabConfig> candidates = roomConfigs.Where(cfg =>
            requiredDoors.All(r => cfg.HasDoor(r)) &&
            forbiddenDoors.All(f => !cfg.HasDoor(f))
        ).ToList();

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    public void ClearLevel()
    {
        foreach (var room in spawnedRooms)
        {
            if (room != null && room.gameObject != null)
                Destroy(room.gameObject);
        }
        spawnedRooms.Clear();
        slotMap.Clear();
    }
}
