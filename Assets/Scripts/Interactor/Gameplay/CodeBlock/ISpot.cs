using UnityEngine;

public interface ISpot
{
    public Transform Transform { get; }
    public Vector2 DropPosition { get; }
    public ICodeBlock Block { get; }
}