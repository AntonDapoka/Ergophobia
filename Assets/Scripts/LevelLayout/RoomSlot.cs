public struct RoomSlot
{
    public int X;
    public int Lane;
    public bool IsStaggered;

    public RoomSlot(int x, int lane, bool staggered = false)
    {
        X = x;
        Lane = lane;
        IsStaggered = staggered;
    }

    public RoomSlot GetNeighbor(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.East: return new RoomSlot(X + 1, Lane, IsStaggered);
            case DoorDirection.West: return new RoomSlot(X - 1, Lane, IsStaggered);
            case DoorDirection.North: return new RoomSlot(X, Lane + 1, IsStaggered);
            case DoorDirection.South: return new RoomSlot(X, Lane - 1, IsStaggered);
            default: return this;
        }
    }

    public override string ToString()
    {
        return $"({X}, {Lane}, staggered={IsStaggered})";
    }
}
