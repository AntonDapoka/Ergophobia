using UnityEngine;

public interface ICodeBlockInputManager
{
    public void OnUpdate();
    public Vector3 ScreenPointerPosition { get; }
    public Vector3 CanvasPointerPosition { get; }
}