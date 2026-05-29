using MG_BlocksEngine2.Core;

namespace MG_BlocksEngine2.Block.Instruction
{
    /// <summary>
    /// No-Op instruction used as a placeholder for empty lines during step execution.
    /// It advances the stack pointer without performing any action.
    /// </summary>
    public class BE2_Ins_EmptyStep : BE2_InstructionBase, I_BE2_Instruction
    {
        public override void Function()
        {
            ExecuteNextInstruction();
        }
    }
}
