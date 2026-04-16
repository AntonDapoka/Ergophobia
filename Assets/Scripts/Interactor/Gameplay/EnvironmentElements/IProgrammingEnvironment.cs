using System.Collections.Generic;
using UnityEngine;

public interface IProgrammingEnvironment
{
    public Transform Transform { get; }
    public List<ICodeBlock> BlocksList { get; }
    public IBehaviourInteractor TargetObject { get; }

    bool Visible { get; set; }  /// Set if the Programming Environment is visible (alpha) and interactable (blocks raycast)
    void UpdateBlocksList();    /// Updates the BlockList with all the blocks in this environment 
    void ClearBlocks();         /// Removes/Destroys all the blocks from the environment
}