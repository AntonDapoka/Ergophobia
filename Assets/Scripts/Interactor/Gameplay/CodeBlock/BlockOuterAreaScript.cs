using UnityEngine;

public abstract class BlockOuterAreaScript
{
    public BlockOuterAreaScript(Transform transform)
    {
        Transform = transform;
        _rectTransform = transform as RectTransform;
        //spotOuterArea = transform.GetComponent<BE2_SpotOuterArea>(); !!!!!!!!!!!!!!!!!!!!

        childBlocksArray = new ICodeBlock[0];

        InitializeLayoutGroup();
    }

    public Transform Transform;
    public RectTransform _rectTransform;
    public ISpot spotOuterArea;
    public int childBlocksCount;
    public ICodeBlock[] childBlocksArray;

    protected virtual void InitializeLayoutGroup() {}

    public virtual Vector2 GetTopDropPosition(ICodeBlock foundBlock)
    {
        return foundBlock.Transform.localPosition + new Vector3(0, (DragDropManagerScript.Instance.GhostBlockTransform as RectTransform).sizeDelta.y - 10, 0);
    }

    public void UpdateChildBlocksList()
    {
        childBlocksArray = new ICodeBlock[0];
        int childCount = Transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            ICodeBlock childBlock = Transform.GetChild(i).GetComponent<ICodeBlock>();
            if (childBlock != null)
            {
                childBlocksArray = ArrayUtilitiesScript.AddReturn(childBlocksArray, childBlock);
            }
        }
        childBlocksCount = childBlocksArray.Length;
    }

    public void UpdateLayout()
    {
        //LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform); !!!!!!!!!!!!!
        UpdateChildBlocksList();
    }
}

