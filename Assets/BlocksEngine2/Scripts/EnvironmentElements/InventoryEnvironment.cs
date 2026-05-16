using UnityEngine;

using MG_BlocksEngine2.Block;

namespace MG_BlocksEngine2.Environment
{
    public class InventoryEnvironment : HolderEnvironment
    {
        public override bool CanPlaceBlock(I_BE2_Block block) => true;
        public override bool CanPickupBlock(I_BE2_Block block) => true;
    }
}
