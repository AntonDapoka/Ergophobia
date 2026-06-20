using MG_BlocksEngine2.Block;

namespace MG_BlocksEngine2.DragDrop
{
    /// <summary>
    /// Optional interface for spots that want to restrict which block types can be dropped into them.
    /// </summary>
    public interface I_BE2_SpotFilter
    {
        /// <summary>
        /// Returns true if the spot can accept the dragged block.
        /// </summary>
        bool CanAcceptBlock(I_BE2_Block block);
    }
}
