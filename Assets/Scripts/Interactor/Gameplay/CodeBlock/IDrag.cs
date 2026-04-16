using UnityEngine;

public interface IDrag
{
   public void OnPointerDown(); // Prepares the block to possibly be dragged
   public void OnRightPointerDownOrHold(); // Opens up the context menu
   public void OnDragStart(); // v2.13 - OnDragStart method added to the I_BE2_Drag interface 
   public void OnDrag(); // Keeps detecting possible drop spots 
   public void OnPointerUp(); // Places the block into a found spot, programming env, or deletes it

   public Transform Transform { get; }
   public Vector2 RayPoint { get; }
   public ICodeBlock Block { get; }
}