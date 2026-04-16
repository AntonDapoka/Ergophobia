using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDropManagerScript : MonoBehaviour //Change from Singleton to smthg else
{
    /*
    private BE2_UI_ContextMenuManager _contextMenuManager;

    // v2.6 - BE2_DragDropManager using instance as property to guarantee return
    private static DragDropManagerScript _instance;
    public static DragDropManagerScript Instance
    {
        get
        {
            if (!_instance) _instance = FindObjectOfType<DragDropManagerScript>();
            return _instance;
        }
        set => _instance = value;
    }

    public IRaycaster Raycaster { get; set; }
    public Transform draggedObjectsTransform;
    public Transform DraggedObjectsTransform => draggedObjectsTransform;
    public IDrag CurrentDrag { get; set; }
    
    // public I_BE2_Spot CurrentSpot { get; set; }
    public RaycasterScript.ConnectionPoint ConnectionPoint { get; set; }
    
    private List<ISpot> _spotsList;
    public List<ISpot> SpotsList
    {
        get
        {
            _spotsList ??= new List<ISpot>();
            return _spotsList;
        }
        set
        {
            _spotsList = value;
        }
    }

    [SerializeField] private Transform _ghostBlock;
    public Transform GhostBlockTransform => _ghostBlock;

    public bool isDragging;
    public float detectionDistance = 40;

    private static Canvas _dragDropComponentsCanvas; //DragDropComponentsCanvas to the Drag and Drop Manager to be used as a reference Canvas 
    public static Canvas DragDropComponentsCanvas
    {
        get
        {
            if (!_dragDropComponentsCanvas) _dragDropComponentsCanvas = Instance.draggedObjectsTransform.GetComponentInParent<Canvas>();
            return _dragDropComponentsCanvas;
        }
    }

    public static bool disableGroupDrag;

    private static void DisableGroupDrag()
    {
        disableGroupDrag = true;
    }

    private static void EnableGroupDrag()
    {
        disableGroupDrag = false;
    }

    private void Awake()
    {
        Raycaster = GetComponent<IRaycaster>();
        _dragDropComponentsCanvas = Instance.draggedObjectsTransform.GetComponentInParent<CanvasScript>().Canvas;
    }

    private void Start()
    {
        _contextMenuManager = BE2_UI_ContextMenuManager.instance;
    }

    private void OnEnable()
    {
        Instance = this;

        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyDown, OnPointerDown);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnSecondaryKeyDown, OnRightPointerDownOrHold);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyHold, OnRightPointerDownOrHold);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnDrag, OnDrag);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyUp, OnPointerUp);

        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnAuxKeyDown, DisableGroupDrag);
        BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnAuxKeyUp, EnableGroupDrag);
    }

    private void OnDisable()
    {
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyDown, OnPointerDown);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnSecondaryKeyDown, OnRightPointerDownOrHold);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyHold, OnRightPointerDownOrHold);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnDrag, OnDrag);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyUp, OnPointerUp);

        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnAuxKeyDown, DisableGroupDrag);
        BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnAuxKeyUp, EnableGroupDrag);
    }

    // v2.13 - BE2_DragDropManager.OnPointerDown made coroutine again to fix issues on using the device simulator
    // v2.12.1 - BE2_DragDropManager.OnPointerDown made non coroutine
    private IEnumerator C_OnPointerDown()
    {
        yield return new WaitForEndOfFrame();

        IDrag drag = Raycaster.GetDragAtPosition(BE2_InputManager.Instance.ScreenPointerPosition);
        if (drag != null)
        {
            CurrentDrag = drag;
            drag.OnPointerDown();
        }
        else
        {
            CurrentDrag = null;
        }
    }
    private void OnPointerDown()
    {
        StartCoroutine(C_OnPointerDown());
    }

    private void OnRightPointerDownOrHold()
    {
        IDrag drag = Raycaster.GetDragAtPosition(BE2_InputManager.Instance.ScreenPointerPosition);
        if (drag != null)
        {
            drag.OnRightPointerDownOrHold();
        }
    }

    private void OnDrag()
    {
        if (CurrentDrag != null)
        {
            // v2.11.1 - added handler method to the BE2_DragDropManager.OnDrag for the new Block drag events
            if (!isDragging)
            {
                // v2.13 - invoke the OnDragStart event 
                CurrentDrag.OnDragStart();
                BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypes.OnDragStart);

                StartCoroutine(C_HandleDragEvents(CurrentDrag.Block));
            }

            CurrentDrag.OnDrag();

            isDragging = true;
        }
    }

    // v2.11.1 - added a handler method on the BE2_DragDropManager.OnPointerUp to call the new Block drop events
    private void OnPointerUp()
    {
        if (CurrentDrag != null && isDragging)
        {
            CurrentDrag.OnPointerUp();
            StartCoroutine(C_HandleDropEvents(CurrentDrag.Block));
        }

        CurrentDrag = null;
        // CurrentSpot = null;
        ConnectionPoint = new BE2_Raycaster.ConnectionPoint();
        GhostBlockTransform.SetParent(null);
        isDragging = false;

        BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypes.OnPrimaryKeyUpEnd);
    }

    // v2.12 - bugfix: drop events not being called correctly, events handler refactored 
    private IEnumerator C_HandleDropEvents(ICodeBlock block)
    {
        yield return new WaitForEndOfFrame();

        BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDrop, block as Object ? block : null);
        BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypes.OnBlockDrop);

        if (block as Object != null)
        {
            block.Instruction.InstructionBase.BlocksStack = block.Transform.GetComponentInParent<I_BE2_BlocksStack>();
            block.ParentSection = block.Transform.GetComponentInParent<I_BE2_BlockSection>();

            if (block.ParentSection == null)
            {
                BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDropAtProgrammingEnv, block);
            }
            else
            {
                if (block.Transform.parent.GetComponent<I_BE2_BlockSectionHeader>() != null)
                {
                    BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDropAtInputSpot, block);
                }
                else
                {
                    BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDropAtStack, block);
                }
            }
        }
        else
        {
            BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDropDestroy, null);
        }
    }

    private IEnumerator C_HandleDragEvents(ICodeBlock block)
    {
        I_BE2_BlockSectionHeader parentHeader = null;
        if (block as Object != null)
        {
            block.Instruction.InstructionBase.BlocksStack = block.Transform.GetComponentInParent<I_BE2_BlocksStack>();
            block.ParentSection = block.Transform.GetComponentInParent<IBlockSection>();
            parentHeader = block.Transform.parent.GetComponent<I_BE2_BlockSectionHeader>();
        }

        yield return new WaitForEndOfFrame();

        BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDragOut, block as Object ? block : null);

        if (block as Object != null)
        {
            if (parentHeader != null)
            {
                BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDragFromInputSpot, block);
            }
            else
            {
                if (block.ParentSection == null)
                {
                    BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDragFromProgrammingEnv, block);
                }
                else
                {
                    BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDragFromStack, block);
                }
            }
        }
        else
        {
            BE2_MainEventsManager.Instance.TriggerEvent(BE2EventTypesBlock.OnDragFromOutside, null);
        }
    }

    public void AddToSpotsList(ISpot spot)
    {
        if (!SpotsList.Contains(spot) && spot != null)
            SpotsList.Add(spot);
    }

    public void RemoveFromSpotsList(ISpot spot)
    {
        if (SpotsList.Contains(spot))
            SpotsList.Remove(spot);
    }*/
}