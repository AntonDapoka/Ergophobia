using CodeblockEntities;

namespace MG_BlocksEngine2.Block.Instruction
{
    public class BE2_Ins_SpeedUp : BE2_Ins_EntityModifierBase, I_BE2_Instruction
    {
        public override void Function()
        {
            ApplyModifier();
            ExecuteNextInstruction();
        }

        protected override void ApplyToEntity(ICodeblockEntity entity, float value)
        {
            entity.ModifySpeed(value);
        }
    }
}
