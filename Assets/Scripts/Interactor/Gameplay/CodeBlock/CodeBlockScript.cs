using UnityEngine;
/*
[RequireComponent(typeof(IBlockLayout))]
[RequireComponent(typeof(IInstruction))]
[RequireComponent(typeof(IDrag))]*/
public class CodeBlockScript : MonoBehaviour//, ICodeBlock
{
   /* [SerializeField] private BlockTypeEnum _type;
    private Transform _transform;

    public BlockTypeEnum Type { get => _type; set => _type = value; }
    public Transform Transform => _transform ? _transform : transform;
    public IBlockLayout Layout { get; set; }
    public IInstruction Instruction { get; set; }
    public IBlockSection ParentSection { get; set; }
    public ICodeBlock ParentBlock { get; set; }
    public IDrag Drag { get; set; }

    private void OnValidate()
    {
        Awake();
    }

    private void Awake()
    {
        _transform = transform;
        Layout = GetComponent<IBlockLayout>();
        Instruction = GetComponent<IInstruction>();
        Drag = GetComponent<IDrag>();
    }

    private void Start()
    {

        GetParentSection();
    }

    // v2.12 - listen to event moved to OnEnable in Block class
    private void OnEnable()
    {
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyUpEnd, GetParentSection);
    }

    private void OnDisable()
    {
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyUpEnd, GetParentSection);
    }

    private void GetParentSection()
    {
        ParentBlock = transform.parent.GetComponentInParent<ICodeBlock>();
        ParentSection = GetComponentInParent<IBlockSection>();
    }

    public void SetShadowActive(bool value)
    {
        if (Type != BlockTypeEnum.operation)
        {
            if (value)
            {
                foreach (IBlockSection section in Layout.SectionsArray)
                {
                    if (section.Header != null && section.Header.Shadow)
                    {
                        section.Header.Shadow.enabled = true;
                    }
                    if (section.Body != null && section.Body.Shadow)
                    {
                        section.Body.Shadow.enabled = true;
                    }
                }
            }
            else
            {
                foreach (IBlockSection section in Layout.SectionsArray)
                {
                    if (section.Header != null && section.Header.Shadow)
                    {
                        section.Header.Shadow.enabled = false;
                    }
                    if (section.Body != null && section.Body.Shadow)
                    {
                        section.Body.Shadow.enabled = false;
                    }
                }
            }
        }
    }*/
}

