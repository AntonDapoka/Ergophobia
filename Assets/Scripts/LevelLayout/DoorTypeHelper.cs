using System.Collections.Generic;

public static class DoorTypeHelper
{
    public static readonly DoorType[] AllTypes = new[]
    {
        DoorType.NorthLeft,
        DoorType.NorthRight,
        DoorType.SouthLeft,
        DoorType.SouthRight,
        DoorType.West,
        DoorType.East
    };

    public static DoorDirection GetDirection(DoorType type)
    {
        switch (type)
        {
            case DoorType.NorthLeft:
            case DoorType.NorthRight:
                return DoorDirection.North;
            case DoorType.SouthLeft:
            case DoorType.SouthRight:
                return DoorDirection.South;
            case DoorType.West:
                return DoorDirection.West;
            case DoorType.East:
                return DoorDirection.East;
            default:
                return DoorDirection.North;
        }
    }

    public static DoorType GetOpposite(DoorType type)
    {
        switch (type)
        {
            case DoorType.NorthLeft: return DoorType.SouthLeft;
            case DoorType.NorthRight: return DoorType.SouthRight;
            case DoorType.SouthLeft: return DoorType.NorthLeft;
            case DoorType.SouthRight: return DoorType.NorthRight;
            case DoorType.West: return DoorType.East;
            case DoorType.East: return DoorType.West;
            default: return type;
        }
    }

    public static bool IsNorth(DoorType type) => GetDirection(type) == DoorDirection.North;
    public static bool IsSouth(DoorType type) => GetDirection(type) == DoorDirection.South;
    public static bool IsWest(DoorType type) => type == DoorType.West;
    public static bool IsEast(DoorType type) => type == DoorType.East;

    public static List<DoorType> GetTypesByDirection(DoorDirection direction)
    {
        List<DoorType> result = new List<DoorType>();
        foreach (DoorType type in AllTypes)
        {
            if (GetDirection(type) == direction)
                result.Add(type);
        }
        return result;
    }

    public static DoorDirection GetOppositeDirection(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North: return DoorDirection.South;
            case DoorDirection.South: return DoorDirection.North;
            case DoorDirection.West: return DoorDirection.East;
            case DoorDirection.East: return DoorDirection.West;
            default: return direction;
        }
    }
}

public enum DoorDirection
{
    North,
    South,
    West,
    East
}
