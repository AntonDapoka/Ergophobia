using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CodeBlockInputManagerScript : MonoBehaviour, ICodeBlockInputManager
{
    static ICodeBlockInputManager _instance;
    public static ICodeBlockInputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // v2.11 - custom InputManagers derived only from the I_BE2_InputManager interface (not from the BE2_InputManager class) can be used 
                foreach (GameObject go in FindObjectsByType<GameObject>())
                {
                    _instance = go.GetComponent<ICodeBlockInputManager>();
                    if (_instance != null)
                        break;
                }
                // _instance = GameObject.FindObjectOfType<BE2_InputManager>() as I_BE2_InputManager;
            }
            return _instance;
        }
        set => _instance = value;
    }

    public KeyCode primaryKey = KeyCode.Mouse0;
    public KeyCode secondaryKey = KeyCode.Mouse1;
    public KeyCode deleteKey = KeyCode.Delete;

    // v2.10 - added new possible key, auxKey0, to the input manager
    public KeyCode auxKey0 = KeyCode.LeftControl;

    public Vector3 ScreenPointerPosition => Input.mousePosition;
    public Vector3 CanvasPointerPosition
    {
        get
        {
            return GetCanvasPointerPosition();
        }
    }

    EventsManagerScript _mainEventsManager;
    DragDropManagerScript _dragDropManager;

    // v2.9 - bugfix: changing the Canvas Render Mode needed recompiling to work correctly 
    InspectorScript _inspector => InspectorScript.Instance;

    float _holdCounter = 0;
    Vector2 _lastPosition;

    // v2.12 - added keycode list to the input manager to improve performance of blocks that use key input 
    public static List<KeyCode> keyCodeList;

    void OnEnable()
    {
        keyCodeList = new List<KeyCode>();
        keyCodeList.AddRange(System.Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>());

        _mainEventsManager = MainEventsManagerScript.Instance;
        _dragDropManager = DragDropManagerScript.Instance;
    }

    public void OnUpdate()
    {
        // pointer 0 down
        if (Input.GetKeyDown(primaryKey))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnPrimaryKeyDown);
        }

        // pointer 1 down or pointer 0 hold
        if (Input.GetKeyDown(secondaryKey))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnSecondaryKeyDown);
        }
        if (_dragDropManager.CurrentDrag != null && !_dragDropManager.isDragging)
        {
            _holdCounter += Time.deltaTime;
            if (_holdCounter > 0.6f)
            {
                _mainEventsManager.TriggerEvent(EventTypes.OnPrimaryKeyHold);
                _holdCounter = 0;
            }
        }

        // pointer 0
        if (Input.GetKey(primaryKey))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnPrimaryKey);
            // v2.6 - using BE2_Pointer as main pointer input source
            float distance = Vector2.Distance(_lastPosition, (Vector2)ScreenPointerPosition);
            /*if (distance > 0.5f && !BE2_UI_ContextMenuManager.instance.isActive)
            {
                _mainEventsManager.TriggerEvent(EventTypes.OnDrag);
            }*/
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        // pointer 0 up
        if (Input.GetKeyUp(primaryKey))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnPrimaryKeyUp);
            _holdCounter = 0;
        }

        // v2.10 - added new events to the input manager
        if (Input.GetKeyUp(secondaryKey))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnSecondaryKeyUp);
        }
        if (Input.GetKeyDown(auxKey0))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnAuxKeyDown);
        }
        if (Input.GetKeyUp(auxKey0))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnAuxKeyUp);
        }

        if (Input.GetKeyDown(deleteKey))
        {
            _mainEventsManager.TriggerEvent(EventTypes.OnDeleteKeyDown);
        }

        _lastPosition = ScreenPointerPosition;
    }

    Vector3 GetCanvasPointerPosition()
    {
        Camera mainCamera = _inspector.Camera;
        if (_inspector.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
        {
            return ScreenPointerPosition;
        }
        else if (_inspector.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
        {
            var screenPoint = ScreenPointerPosition;
            screenPoint.z = DragDropManagerScript.DragDropComponentsCanvas.transform.position.z - mainCamera.transform.position.z; //distance of the plane from the camera
            return GetMouseInCanvas(screenPoint);
        }
        else if (_inspector.CanvasRenderMode == RenderMode.WorldSpace)
        {
            var screenPoint = ScreenPointerPosition;
            screenPoint.z = DragDropManagerScript.DragDropComponentsCanvas.transform.position.z - mainCamera.transform.position.z; //distance of the plane from the camera
            return GetMouseInCanvas(screenPoint);
        }

        return Vector3.zero;
    }

    Vector3 GetMouseInCanvas(Vector3 position)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            DragDropManagerScript.DragDropComponentsCanvas.transform as RectTransform,
            position,
            _inspector.Camera,
            out Vector3 mousePosition
        );
        return mousePosition;
    }
}
