using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Block.Instruction;

namespace MG_BlocksEngine2.DragDrop
{
    /// <summary>
    /// Input spot that only accepts operation blocks representing a prefab (BE2_Op_Prefab).
    /// </summary>
    public class BE2_SpotBlockPrefabInput : BE2_SpotBlockInput, I_BE2_SpotFilter
    {
        public bool CanAcceptBlock(I_BE2_Block block)
        {
            if (block == null || block.Transform == null)
                return false;

            // Only accept blocks that carry a prefab payload
            return block.Transform.GetComponent<BE2_Op_Prefab>() != null;
        }
    }
}
