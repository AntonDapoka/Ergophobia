using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgrammingEnvironmentScript  : MonoBehaviour, IProgrammingEnvironment
{
    private Transform _transform;
    private CanvasGroup _parentCanvasGroup;
    private GraphicRaycaster _parentGraphicRaycaster;
    [SerializeField] private bool _visible = true;

    public PlayerBehaviourInteractorScript targetObject;

    public Transform Transform => _transform ? _transform : transform;
    public List<ICodeBlock> BlocksList { get; set; }
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
        //DragDropManagerScript.Instance.Raycaster.AddRaycaster(_parentGraphicRaycaster);
    }

    public void UpdateBlocksList()
    {
        BlocksList = new List<ICodeBlock>();
        foreach (Transform child in Transform)
        {
            if (child.gameObject.activeSelf)
            {
                if (child.TryGetComponent<ICodeBlock>(out var childBlock))
                    BlocksList.Add(childBlock);
            }
        }
    }

    /*public void OpenContextMenu() //I did comment it
    {
        BE2_UI_ContextMenuManager.instance.OpenContextMenu(1, this);
    }*/

    public void ClearBlocks()
    {
        BlocksList = new List<ICodeBlock>();
        foreach (Transform child in Transform)
        {
            if (child.gameObject.activeSelf)
            {
                ICodeBlock childBlock = child.GetComponent<ICodeBlock>();
                if (childBlock != null)
                    Destroy(childBlock.Transform.gameObject);
            }
        }

        UpdateBlocksList();
    }
}
