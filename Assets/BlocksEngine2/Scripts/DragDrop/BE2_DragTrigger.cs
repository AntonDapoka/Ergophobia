
using UnityEngine;

namespace MG_BlocksEngine2.DragDrop
{
    // Line-based system: BE2_DragTrigger is now a thin wrapper around BE2_DragBlock.
    // The only behavioral difference (execution-manager registration for triggers)
    // is handled inside BE2_DragBlock.OnPointerUp via a BlockTypeEnum.trigger check.
    public class BE2_DragTrigger : BE2_DragBlock
    {
    }
}
