using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// namespace MG_BlocksEngine2.Environment

public class ProgrammingEnvironmentScript  : MonoBehaviour, IProgrammingEnvironment
{
    private Transform _transform;
    private CanvasGroup _parentCanvasGroup;
    private GraphicRaycaster _parentGraphicRaycaster;
    [SerializeField] private bool _visible = true;

    public PlayerBehaviourInteractorScript targetObject;

    public Transform Transform => _transform ? _transform : transform;
    public List<I_BE2_Block> BlocksList { get; set; }
    public IBehaviourInteractor TargetObject => targetObject;

    public bool Visible 
    {
        get => _visible;
        set
        {
            _visible = value;
            if (gameObject.scene.name != null && _parentCanvasGroup)
            {
                if (value)
                {
                    _parentCanvasGroup.alpha = 1;
                    _parentCanvasGroup.blocksRaycasts = true;
                }
                else
                {
                    _parentCanvasGroup.alpha = 0;
                    _parentCanvasGroup.blocksRaycasts = false;
                }
            }
        }
    }

    private void OnValidate()
    {
        _parentCanvasGroup = GetComponentInParent<CanvasGroup>();
        Visible = _visible;
    }

    private void Awake()
    {
        if (targetObject)
            targetObject.ProgrammingEnv = this;
        
        _transform = transform;
        UpdateBlocksList();

        _parentCanvasGroup = GetComponentInParent<CanvasGroup>();
        _parentGraphicRaycaster = _parentCanvasGroup.GetComponent<GraphicRaycaster>();
    }

    private void Start()
    {
        BE2_DragDropManager.Instance.Raycaster.AddRaycaster(_parentGraphicRaycaster);
    }

    public void UpdateBlocksList()
    {
        BlocksList = new List<I_BE2_Block>();
        foreach (Transform child in Transform)
        {
            if (child.gameObject.activeSelf)
            {
                I_BE2_Block childBlock = child.GetComponent<I_BE2_Block>();
                if (childBlock != null)
                    BlocksList.Add(childBlock);
            }
        }
    }

    public void OpenContextMenu()
    {
        BE2_UI_ContextMenuManager.instance.OpenContextMenu(1, this);
    }

    public void ClearBlocks()
    {
        BlocksList = new List<I_BE2_Block>();
        foreach (Transform child in Transform)
        {
            if (child.gameObject.activeSelf)
            {
                I_BE2_Block childBlock = child.GetComponent<I_BE2_Block>();
                if (childBlock != null)
                    Destroy(childBlock.Transform.gameObject);
            }
        }

        UpdateBlocksList();
    }
}
