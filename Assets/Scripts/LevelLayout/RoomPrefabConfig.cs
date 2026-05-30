using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomConfig", menuName = "Level Generation/Room Config")]
public class RoomPrefabConfig : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Available Doors")]
    public bool hasNorthLeft;
    public bool hasNorthRight;
    public bool hasSouthLeft;
    public bool hasSouthRight;
    public bool hasWest;
    public bool hasEast;

    [Header("Blocked Doors")]
    public List<DoorType> blockedDoors = new List<DoorType>();

    public bool HasDoor(DoorType type)
    {
        if (blockedDoors.Contains(type)) return false;

        switch (type)
        {
            case DoorType.NorthLeft: return hasNorthLeft;
            case DoorType.NorthRight: return hasNorthRight;
            case DoorType.SouthLeft: return hasSouthLeft;
            case DoorType.SouthRight: return hasSouthRight;
            case DoorType.West: return hasWest;
            case DoorType.East: return hasEast;
            default: return false;
        }
    }

    public int GetAvailableDoorCount()
    {
        int count = 0;
        foreach (DoorType type in DoorTypeHelper.AllTypes)
        {
            if (HasDoor(type)) count++;
        }
        return count;
    }

    public List<DoorType> GetAvailableDoors()
    {
        List<DoorType> result = new List<DoorType>();
        foreach (DoorType type in DoorTypeHelper.AllTypes)
        {
            if (HasDoor(type)) result.Add(type);
        }
        return result;
    }
}
