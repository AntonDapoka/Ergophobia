using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObject/Block")]
public class BlockScript : ScriptableObject
{
    [Header("Only Gameplay")]
    public TileBase tile;
    public BlockType typeBlock;
    public ActionType typeAction;
    public Vector2Int range = new(5, 4);

    [Header("Only UI")]
    public bool isStackable = true;
    public Sprite sprite;

}

public enum BlockType
{
    Start,
    Write,
    End,
    Shoot,
    If,
    Elif,
    Else,
    For,
    BulletBasic,
    BulletFire,
    BulletIce,
    BulletRandom,

    Health,
    Size,
    Count,
    Damage,
    Speed,
    Increment,
    Decrement,

    Method,

    Break

}

public enum ActionType
{
    Construction,
    Variable,
    Type
}
