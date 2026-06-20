using CodeblockEntities;

namespace MG_BlocksEngine2.Block.Instruction
{
    public class BE2_Op_Player : BE2_Op_EntityBase, I_BE2_Instruction
    {
        public override CodeblockEntityType EntityType => CodeblockEntityType.Player;
    }
}
