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
        return direction switch
        {
            DoorDirection.East => new RoomSlot(X + 1, Lane, IsStaggered),
            DoorDirection.West => new RoomSlot(X - 1, Lane, IsStaggered),
            DoorDirection.North => new RoomSlot(X, Lane + 1, IsStaggered),
            DoorDirection.South => new RoomSlot(X, Lane - 1, IsStaggered),
            _ => this,
        };
   }

    public override string ToString()
    {
        return $"({X}, {Lane}, staggered={IsStaggered})";
    }
}
