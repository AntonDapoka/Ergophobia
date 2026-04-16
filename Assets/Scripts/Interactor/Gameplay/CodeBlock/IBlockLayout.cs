using UnityEngine;

public interface IBlockLayout
{
    public RectTransform RectTransform { get; set; }
    public IBlockSection[] SectionsArray { get; }
    public BlockOuterAreaScript OuterArea { get; set; } // OuterArea reference to the block layout so block can be dragged with the under blocks

    public Color Color { get; set; } // Block visible color
    public Vector2 Size { get; } // Returns the size of the whole block. Headers and Bodies with child blocks are counted on
    public void UpdateLayout() {} // Updates the layout of the block. Used to correctly resize the blocks after adding child and operation blocks
}
