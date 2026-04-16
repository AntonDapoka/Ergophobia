using UnityEngine;

public interface IInstruction
{
   public IInstructionBase InstructionBase { get; }
 
   /// <summary>
   /// Default value is false. Set true to force the Instruction to be called in the update method of the Execution Manager.
   /// This property should be set true for blocks that contain timers.
   /// ex.: wait, lerp in a fixed time period (block slide forward)
   /// </summary>
   public bool ExecuteInUpdate { get; }

   public string Operation(); // Used to implement the logic of operation blocks
   public void Function(); // Used to implement the logic of trigger, simple, condition and loop blocks
   public void Reset(); // Reset method to the instructions to enable reuse by Function Blocks
}
