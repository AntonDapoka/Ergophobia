using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.DragDrop;

namespace MG_BlocksEngine2.Block.Instruction
{
    /// <summary>
    /// Input spot that only accepts entity code blocks (Enemy, Player, Bullet).
    /// </summary>
    public class BE2_SpotBlockEntityInput : BE2_SpotBlockInput, I_BE2_SpotFilter
    {
        public bool CanAcceptBlock(I_BE2_Block block)
        {
            if (block == null || block.Transform == null)
                return false;

            return block.Transform.GetComponent<BE2_Op_EntityBase>() != null;
        }
    }
}
