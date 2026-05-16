using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.EditorScript;
using MG_BlocksEngine2.Utils;
using MG_BlocksEngine2.Environment;
using MG_BlocksEngine2.Core;

namespace MG_BlocksEngine2.DragDrop
{
    public class BE2_Raycaster : MonoBehaviour, I_BE2_Raycaster
    {
        BE2_DragDropManager _dragDropManager;
        PointerEventData _pointerEventData;

        [SerializeField]
        GraphicRaycaster[] raycasters;

        [SerializeField]
        EventSystem eventSystem;

        void Awake()
        {
            _dragDropManager = GetComponent<BE2_DragDropManager>();
        }

        // v2.4 - added methods to add/remove raycasters from the BE2 Raycaster component
        public GraphicRaycaster[] AddRaycaster(GraphicRaycaster raycaster = null)
        {
            if (raycaster != null)
            {
                if (BE2_ArrayUtils.Find(ref raycasters, (x => x == raycaster)) == null)
                {
                    BE2_ArrayUtils.Add(ref raycasters, raycaster);
                }
            }

            return raycasters;
        }
        public GraphicRaycaster[] RemoveRaycaster(GraphicRaycaster raycaster)
        {
            if (BE2_ArrayUtils.Find(ref raycasters, (x => x == raycaster)) != null)
            {
                BE2_ArrayUtils.Remove(ref raycasters, raycaster);
            }

            return raycasters;
        }

        public I_BE2_Drag GetDragAtPosition(Vector2 position)
        {
            _pointerEventData = new PointerEventData(eventSystem);

            // v2.6 - Raycaster ray position adjusted base on the Canvas render mode
            if (BE2_Inspector.Instance.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                _pointerEventData.position = position;
            }
            else
            {
                _pointerEventData.position = BE2_Inspector.Instance.Camera.WorldToScreenPoint(BE2_Pointer.Instance.transform.position);
            }

            List<RaycastResult> globalResults = new List<RaycastResult>();
            int rayCount = raycasters.Length;
            for (int i = 0; i < rayCount; i++)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                raycasters[i].Raycast(_pointerEventData, results);
                globalResults.AddRange(results);
            }

            int resultCount = globalResults.Count;
            for (int i = 0; i < resultCount; i++)
            {
                GameObject resultGameObject = globalResults[i].gameObject;

                I_BE2_Drag drag = resultGameObject.GetComponentInParent<I_BE2_Drag>();
                if (drag != null)
                {
                    return drag;
                }
            }

            return null;
        }

        public I_BE2_Spot GetSpotAtPosition(Vector3 position)
        {
            _pointerEventData = new PointerEventData(eventSystem);

            // v2.6 - Raycaster ray position adjusted base on the Canvas render mode
            if (BE2_Inspector.Instance.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                _pointerEventData.position = position;
            }
            else
            {
                _pointerEventData.position = BE2_Inspector.Instance.Camera.WorldToScreenPoint(BE2_Pointer.Instance.transform.position);
            }

            List<RaycastResult> globalResults = new List<RaycastResult>();
            int rayCount = raycasters.Length;
            for (int i = 0; i < rayCount; i++)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                raycasters[i].Raycast(_pointerEventData, results);
                globalResults.AddRange(results);
            }

            int resultCount = globalResults.Count;
            for (int i = 0; i < resultCount; i++)
            {
                RaycastResult result = globalResults[i];
                if (result.gameObject.activeSelf)
                {
                    I_BE2_Spot spot = result.gameObject.GetComponent<I_BE2_Spot>();
                    if (spot != null)
                    {
                        return spot;
                    }
                }
            }

            return null;
        }

        public I_BE2_Spot FindClosestSpotOfType<T>(I_BE2_Drag drag, float maxDistance)
        {
            float minDistance = Mathf.Infinity;
            I_BE2_Spot foundSpot = null;
            int spotsCount = _dragDropManager.SpotsList.Count;
            for (int i = 0; i < spotsCount; i++)
            {
                I_BE2_Spot spot = _dragDropManager.SpotsList[i];
                if (spot is T && spot.Transform.gameObject.activeSelf)
                {
                    I_BE2_Drag d = spot.Transform.GetComponentInParent<I_BE2_Drag>();

                    // v2.4 - added programming env check to the BE2_Raycaster to verify if the block is placed in a visible or hidden environment 
                    I_BE2_ProgrammingEnv programmingEnv = d.Transform.GetComponentInParent<BE2_ProgrammingEnv>();

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
            public I_BE2_Spot spot; // to connect the dragged block
            public I_BE2_Block block; // to be connected to the dragged block
        }

        // Line-based system: find the closest empty line for dropping a block
        public BE2_Line FindClosestEmptyLine(I_BE2_Drag drag, float maxDistance)
        {
            // First pass: find a line that contains the drag point in its bounds
            int spotsCount = _dragDropManager.SpotsList.Count;
            for (int i = 0; i < spotsCount; i++)
            {
                I_BE2_Spot spot = _dragDropManager.SpotsList[i];
                if (spot is BE2_Line line && !line.IsOccupied && line.Transform.gameObject.activeSelf)
                {
                    I_BE2_ProgrammingEnv programmingEnv = line.Transform.GetComponentInParent<BE2_ProgrammingEnv>();
                    if (programmingEnv == null || !programmingEnv.Visible)
                        continue;

                    if (IsPointInsideLine(line, drag.RayPoint))
                        return line;
                }
            }

            // Fallback: find closest line by distance to center
            float minDistance = Mathf.Infinity;
            BE2_Line foundLine = null;
            for (int i = 0; i < spotsCount; i++)
            {
                I_BE2_Spot spot = _dragDropManager.SpotsList[i];
                if (spot is BE2_Line line && !line.IsOccupied && line.Transform.gameObject.activeSelf)
                {
                    I_BE2_ProgrammingEnv programmingEnv = line.Transform.GetComponentInParent<BE2_ProgrammingEnv>();
                    if (programmingEnv == null || !programmingEnv.Visible)
                        continue;

                    float distance = Vector2.Distance(drag.RayPoint, spot.DropPosition);
                    if (distance < minDistance && distance <= maxDistance)
                    {
                        foundLine = line;
                        minDistance = distance;
                    }
                }
            }

            return foundLine;
        }

        bool IsPointInsideLine(BE2_Line line, Vector2 worldPoint)
        {
            RectTransform rt = line.Transform as RectTransform;
            if (rt == null) return false;

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            return worldPoint.x >= minX && worldPoint.x <= maxX && worldPoint.y >= minY && worldPoint.y <= maxY;
        }

        public HolderEnvironment FindHolderEnvironmentAtPoint(Vector2 worldPoint)
        {
            foreach (HolderEnvironment holder in HolderEnvironment.ActiveHolders)
            {
                if (holder == null) continue;
                if (!holder.gameObject.activeInHierarchy) continue;
                if (holder.ContainsPoint(worldPoint))
                    return holder;
            }
            return null;
        }
    }
}
