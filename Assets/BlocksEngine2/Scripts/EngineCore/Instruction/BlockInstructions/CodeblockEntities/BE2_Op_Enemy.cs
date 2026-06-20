using CodeblockEntities;

namespace MG_BlocksEngine2.Block.Instruction
{
    public class BE2_Op_Enemy : BE2_Op_EntityBase, I_BE2_Instruction
    {
        public override CodeblockEntityType EntityType => CodeblockEntityType.Enemy;
    }
}
