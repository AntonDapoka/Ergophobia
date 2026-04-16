using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(DragDropManagerScript))]
public class RaycasterScript : MonoBehaviour, IRaycaster
{
    [SerializeField] private GraphicRaycaster[] raycasters;
    [SerializeField] private EventSystem eventSystem;

    private DragDropManagerScript _dragDropManager;
    private PointerEventData _pointerEventData;

    private void Awake()
    {
        _dragDropManager = GetComponent<DragDropManagerScript>();
    }

    public GraphicRaycaster[] AddRaycaster(GraphicRaycaster raycaster = null)
    {
        if (raycaster != null)
        {
            if (ArrayUtilitiesScript.Find(ref raycasters, x => x == raycaster) == null)
                ArrayUtilitiesScript.Add(ref raycasters, raycaster);
        }
        return raycasters;
    }

    public GraphicRaycaster[] RemoveRaycaster(GraphicRaycaster raycaster)
    {
        if (ArrayUtilitiesScript.Find(ref raycasters, x => x == raycaster) != null)
            ArrayUtilitiesScript.Remove(ref raycasters, raycaster);

        return raycasters;
    }

    public IDrag GetDragAtPosition(Vector2 position)
    {
        _pointerEventData = new PointerEventData(eventSystem);

        if (InspectorScript.Instance.CanvasRenderMode == RenderMode.ScreenSpaceOverlay) // Raycaster ray position adjusted base on the Canvas render mode
            _pointerEventData.position = position;
        else
            _pointerEventData.position = InspectorScript.Instance.Camera.WorldToScreenPoint(PointerScript.Instance.transform.position);

        List<RaycastResult> globalResults = new();
        int rayCount = raycasters.Length;
        for (int i = 0; i < rayCount; i++)
        {
            List<RaycastResult> results = new();
            raycasters[i].Raycast(_pointerEventData, results);
            globalResults.AddRange(results);
        }

        int resultCount = globalResults.Count;
        for (int i = 0; i < resultCount; i++)
        {
            GameObject resultGameObject = globalResults[i].gameObject;
            IDrag drag = resultGameObject.GetComponentInParent<IDrag>();
            if (drag != null)
                return drag;
        }
        return null;
    }

    public ISpot GetSpotAtPosition(Vector3 position)
    {
        _pointerEventData = new PointerEventData(eventSystem);

        // v2.6 - Raycaster ray position adjusted base on the Canvas render mode
        if (InspectorScript.Instance.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
        {
            _pointerEventData.position = position;
        }
        else
        {
            _pointerEventData.position = InspectorScript.Instance.Camera.WorldToScreenPoint(PointerScript.Instance.transform.position);
        }

        List<RaycastResult> globalResults = new();
        int rayCount = raycasters.Length;
        for (int i = 0; i < rayCount; i++)
        {
            List<RaycastResult> results = new();
            raycasters[i].Raycast(_pointerEventData, results);
            globalResults.AddRange(results);
        }

        int resultCount = globalResults.Count;
        for (int i = 0; i < resultCount; i++)
        {
            RaycastResult result = globalResults[i];
            if (result.gameObject.activeSelf)
            {
                ISpot spot = result.gameObject.GetComponent<ISpot>();
                if (spot != null)
                    return spot;
            }
        }

        return null;
    }

    public ISpot FindClosestSpotOfType<T>(IDrag drag, float maxDistance)
    {
        float minDistance = Mathf.Infinity;
        ISpot foundSpot = null;
        int spotsCount = _dragDropManager.SpotsList.Count;
        for (int i = 0; i < spotsCount; i++)
        {
            ISpot spot = _dragDropManager.SpotsList[i];
            if (spot is T && spot.Transform.gameObject.activeSelf)
            {
                IDrag d = spot.Transform.GetComponentInParent<IDrag>();

                // v2.4 - added programming env check to the BE2_Raycaster to verify if the block is placed in a visible or hidden environment 
                IProgrammingEnvironment programmingEnv = d.Transform.GetComponentInParent<ProgrammingEnvironmentScript>();

                // v2.12.1 - bugfix: null exception on draggin operation blocks with other operations as input
                if (d != drag && programmingEnv != null && programmingEnv.Visible)
                {
                    float distance = Vector2.Distance(drag.RayPoint, spot.DropPosition);
                    if (distance < minDistance && distance <= maxDistance)
                    {
                        foundSpot = spot;
                        minDistance = distance;
                    }
                }
            }
        }

        return foundSpot;
    }

    public struct ConnectionPoint
    {
        public ISpot spot; // to connect the dragged block
        public ICodeBlock block; // to be connected to the dragged block
    }

    // for non operation blocks
    public ISpot FindClosestConnectableSpot(IDrag drag, float maxDistance)
    {
        float minDistance = Mathf.Infinity;
        ConnectionPoint connectionPoint = new();
        connectionPoint.spot = null;
        int spotsCount = _dragDropManager.SpotsList.Count;
        for (int i = 0; i < spotsCount; i++)
        {
            ISpot spot = _dragDropManager.SpotsList[i];
            IProgrammingEnvironment programmingEnv = spot.Transform.GetComponentInParent<IProgrammingEnvironment>();
            // v2.13.1 - bugfix: blocks being dropped on spots from not visible environments
            if (programmingEnv == null || !programmingEnv.Visible)
                continue;

            if (spot.Transform.gameObject.activeSelf)
            {
                if (drag.Block == spot.Block)
                    continue;

                float distance = Vector2.Distance(drag.RayPoint, spot.DropPosition);
                if (distance < minDistance && distance <= maxDistance)
                {
                    connectionPoint.spot = spot;
                    minDistance = distance;
                }
            }
        }

        return connectionPoint.spot;
    }

    // for non operation blocks
    public ICodeBlock FindClosestConnectableBlock(IDrag drag, float maxDistance)
    {
        float minDistance = Mathf.Infinity;
        ConnectionPoint connectionPoint = new ConnectionPoint();
        connectionPoint.spot = null;

        IProgrammingEnvironment activeEnv = null;
        foreach (IProgrammingEnvironment env in ExecutionManagerScript.Instance.ProgrammingEnvsList)
        {
            if (env.Visible)
            {
                activeEnv = env;
                break;
            }
        }

        if (activeEnv != null)
        {
            Vector3 point = drag.Block.Layout.OuterArea.Transform.position;
            if (drag.Block.Layout.OuterArea.childBlocksCount > 0)
            {
                point = drag.Block.Layout.OuterArea.childBlocksArray[drag.Block.Layout.OuterArea.childBlocksCount - 1].Layout.OuterArea.Transform.position;
            }

            foreach (Transform child in activeEnv.Transform)
            {
                ICodeBlock block = child.GetComponent<ICodeBlock>();
                if (block != null && block is not GhostBlockScript && block.Type != BlockType.define && block.Type != BlockType.operation && block.Type != BlockType.trigger)
                {
                    float distance = Vector2.Distance(point, block.Drag.RayPoint);
                    if (distance < minDistance && distance <= maxDistance)
                    {
                        connectionPoint.spot = null;
                        connectionPoint.block = block;
                        minDistance = distance;
                    }
                }
            }
        }

        return connectionPoint.block;
    }
}

