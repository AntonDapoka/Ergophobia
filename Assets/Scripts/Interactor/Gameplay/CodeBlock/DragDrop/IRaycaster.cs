using UnityEngine;
using UnityEngine.UI;

public interface IRaycaster
{
    public GraphicRaycaster[] AddRaycaster(GraphicRaycaster raycaster);// Adds a canvas raycaster to the list of canvases to be looked up when parforming the BE2 pointer actions
    public GraphicRaycaster[] RemoveRaycaster(GraphicRaycaster raycaster); // Removes a canvas raycaster from the list making the BE2 actions, as drag and drop, not be performed on the regarding canvas // <param name="raycaster"></param>

    public IDrag GetDragAtPosition(Vector2 position); // Returns the first draggable component (blocks by default) at the position 

    public ISpot GetSpotAtPosition(Vector3 position); // Returns the first spot component (used to place draggable components at) at the position
    public ISpot FindClosestSpotOfType<T>(IDrag drag, float maxDistance); // Returns the first spot component of an specific type (used to place draggable components at) that is closer to the given draggable and inside the range
    // public ISpot FindClosestSpotForBlock(IDrag drag, float maxDistance);// Returns the first spot component of types BE2_SpotBlockBody or BE2_SpotOuterArea (used to place draggable components at) that is closer to the given draggable and inside the range
}
