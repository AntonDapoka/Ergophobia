using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLayoutGenerationScript : MonoBehaviour
{
    private const float RoomWidth = 16f;
    private const float RoomHeight = 9f;

    [Header("Systems")]
    [SerializeField] private RoomPool roomPool;

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
    [SerializeField] private int roomsPerFrame = 1;

    [Header("Debug")]
    [SerializeField] private bool generateOnStart = true;

    private List<RoomScript> spawnedRooms = new List<RoomScript>();
    private Dictionary<RoomSlot, RoomScript> slotMap = new Dictionary<RoomSlot, RoomScript>();
    private List<System.Tuple<RoomScript, DoorDirection>> pendingBranches = new List<System.Tuple<RoomScript, DoorDirection>>();

    private void Start()
    {
        if (generateOnStart)
            GenerateLevel();
    }

    public void GenerateLevel()
    {
        StartCoroutine(GenerateLevelAsync());
    }

    public IEnumerator GenerateLevelAsync()
    {
        ClearLevel();

        if (roomPool == null)
        {
            Debug.LogError("RoomPool is not assigned!");
            yield break;
        }

        if (roomConfigs == null || roomConfigs.Count == 0)
        {
            Debug.LogError("No room configs assigned!");
            yield break;
        }

        yield return StartCoroutine(roomPool.InitializeAsync(roomConfigs));

        RoomPrefabConfig startConfig = PickStartConfig();
        if (startConfig == null)
        {
            Debug.LogError("Failed to pick start room config.");
            yield break;
        }

        RoomScript startRoom = null;
        yield return StartCoroutine(SpawnRoomInSlot(startConfig, new RoomSlot(0, 0), r => startRoom = r));

        if (startRoom == null)
        {
            Debug.LogError("Failed to spawn start room.");
            yield break;
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

            RoomScript newRoom = null;
            yield return StartCoroutine(SpawnRoomInSlot(config, mainSlot, r => newRoom = r));
            if (newRoom == null) break;

            spawnedRooms.Add(newRoom);
            slotMap[mainSlot] = newRoom;

            ConnectSlots(previousMainRoom.CurrentSlot, mainSlot, DoorDirection.East);

            pendingBranches.Add(new System.Tuple<RoomScript, DoorDirection>(newRoom, DoorDirection.North));
            pendingBranches.Add(new System.Tuple<RoomScript, DoorDirection>(newRoom, DoorDirection.South));

            if (i % roomsPerFrame == 0)
                yield return null;

            previousMainRoom = newRoom;
        }

        for (int i = 0; i < pendingBranches.Count; i++)
        {
            var branch = pendingBranches[i];

            if (allowAlignedBranches)
                yield return StartCoroutine(TrySpawnBranchAsync(branch.Item1, branch.Item2, false));

            if (allowStaggeredBranches)
                yield return StartCoroutine(TrySpawnBranchAsync(branch.Item1, branch.Item2, true));

            if (i % roomsPerFrame == 0)
                yield return null;
        }

        foreach (var room in spawnedRooms)
        {
            if (room != null)
                room.SealUnusedDoors();
        }

        foreach (var room in spawnedRooms)
        {
            PropsActivator activator = room.GetComponent<PropsActivator>();
            if (activator != null)
                yield return StartCoroutine(activator.ActivateAsync());
        }
    }

    private RoomPrefabConfig PickStartConfig()
    {
        RoomPrefabConfig config = startRoomConfig;

        if (config != null && config.roomPrefabReference != null && config.roomPrefabReference.RuntimeKeyIsValid())
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

        return config;
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

    private IEnumerator SpawnRoomInSlot(RoomPrefabConfig config, RoomSlot slot, System.Action<RoomScript> onComplete)
    {
        RoomScript room = null;
        yield return StartCoroutine(roomPool.GetAsync(config, transform, go => room = go?.GetComponent<RoomScript>()));

        if (room == null)
        {
            Debug.LogError($"Failed to spawn room from config '{config.name}'.");
            onComplete?.Invoke(null);
            yield break;
        }

        room.transform.position = GetSlotPosition(slot);
        room.transform.rotation = Quaternion.identity;
        room.Initialize(slot);
        room.SetDoorsFromConfig(config);
        room.SourceConfig = config;

        PropsActivator activator = room.GetComponent<PropsActivator>();
        if (activator != null)
            activator.Prepare();

        onComplete?.Invoke(room);
    }

    private Vector3 GetSlotPosition(RoomSlot slot)
    {
        float stepX = RoomWidth + roomGapX;
        float stepZ = RoomHeight + roomGapZ;
        float offsetX = (slot.Lane != 0 && slot.IsStaggered) ? -stepX * 0.5f : 0f;
        return new Vector3(-slot.X * stepX + offsetX, 0f, -slot.Lane * stepZ);
    }

    private IEnumerator TrySpawnBranchAsync(RoomScript parentRoom, DoorDirection direction, bool staggered)
    {
        if (Random.value > branchChance) yield break;
        if (!parentRoom.HasAvailableExit(direction)) yield break;

        int lane = direction == DoorDirection.North ? 1 : -1;
        RoomSlot branchSlot = new RoomSlot(parentRoom.CurrentSlot.X, lane, staggered);

        if (IsBranchSlotBlocked(branchSlot)) yield break;

        DoorDirection opposite = DoorTypeHelper.GetOppositeDirection(direction);
        DoorType[] requiredTypes = DoorTypeHelper.GetTypesByDirection(opposite).ToArray();

        // Для любой ветки предпочитаем комнаты с двумя дверями (SouthLeft+SouthRight / NorthLeft+NorthRight)
        List<RoomPrefabConfig> candidates = roomConfigs.Where(c => requiredTypes.All(t => c.HasDoor(t))).ToList();

        if (candidates.Count == 0)
        {
            if (staggered)
                Debug.LogWarning($"No prefab with both {opposite} doors for staggered {direction} branch. Falling back to single-door prefabs.");
            candidates = roomConfigs.Where(c => requiredTypes.Any(t => c.HasDoor(t))).ToList();
        }

        if (candidates.Count == 0) yield break;

        RoomPrefabConfig config = candidates[Random.Range(0, candidates.Count)];

        RoomScript branchRoom = null;
        yield return StartCoroutine(SpawnRoomInSlot(config, branchSlot, r => branchRoom = r));
        if (branchRoom == null) yield break;

        spawnedRooms.Add(branchRoom);
        slotMap[branchSlot] = branchRoom;

        if (staggered)
        {
            ConnectStaggeredBranch(branchRoom, parentRoom, direction);
        }
        else
        {
            ConnectAlignedBranch(branchRoom, parentRoom, direction);
        }
    }

    private void ConnectAlignedBranch(RoomScript branchRoom, RoomScript parentRoom, DoorDirection direction)
    {
        if (direction == DoorDirection.North)
        {
            // Соединяем обе пары дверей: левую с левой, правую с правой
            TryConnectSpecificDoors(branchRoom, DoorType.SouthLeft, parentRoom, DoorType.NorthLeft);
            TryConnectSpecificDoors(branchRoom, DoorType.SouthRight, parentRoom, DoorType.NorthRight);
        }
        else // South
        {
            TryConnectSpecificDoors(branchRoom, DoorType.NorthLeft, parentRoom, DoorType.SouthLeft);
            TryConnectSpecificDoors(branchRoom, DoorType.NorthRight, parentRoom, DoorType.SouthRight);
        }
    }

    private void ConnectStaggeredBranch(RoomScript branchRoom, RoomScript parentRoom, DoorDirection direction)
    {
        int parentX = parentRoom.CurrentSlot.X;
        RoomSlot leftMainSlot = new RoomSlot(parentX + 1, 0);

        if (direction == DoorDirection.North)
        {
            // Правая основная (parent) справа от ветки: SouthRight ↔ NorthLeft
            TryConnectSpecificDoors(branchRoom, DoorType.SouthRight, parentRoom, DoorType.NorthLeft);

            // Левая основная слева от ветки: SouthLeft ↔ NorthRight
            if (slotMap.ContainsKey(leftMainSlot))
                TryConnectSpecificDoors(branchRoom, DoorType.SouthLeft, slotMap[leftMainSlot], DoorType.NorthRight);
        }
        else // South
        {
            // Правая основная (parent) справа от ветки: NorthRight ↔ SouthLeft
            TryConnectSpecificDoors(branchRoom, DoorType.NorthRight, parentRoom, DoorType.SouthLeft);

            // Левая основная слева от ветки: NorthLeft ↔ SouthRight
            if (slotMap.ContainsKey(leftMainSlot))
                TryConnectSpecificDoors(branchRoom, DoorType.NorthLeft, slotMap[leftMainSlot], DoorType.SouthRight);
        }
    }

    private void TryConnectSpecificDoors(RoomScript roomA, DoorType typeA, RoomScript roomB, DoorType typeB)
    {
        DoorScript doorA = roomA.GetDoor(typeA);
        DoorScript doorB = roomB.GetDoor(typeB);

        if (doorA == null || doorB == null) return;
        if (doorA.IsConnected || doorB.IsConnected || doorA.IsBlockedByPrefab || doorB.IsBlockedByPrefab) return;

        doorA.ConnectTo(doorB);
        doorB.ConnectTo(doorA);
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
            cfg.roomPrefabReference != null &&
            cfg.roomPrefabReference.RuntimeKeyIsValid() &&
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
            if (room == null || room.gameObject == null) continue;

            if (room.SourceConfig != null && roomPool != null)
                roomPool.Return(room.SourceConfig, room.gameObject);
            else
                Destroy(room.gameObject);
        }

        spawnedRooms.Clear();
        slotMap.Clear();
        pendingBranches.Clear();
    }
}
