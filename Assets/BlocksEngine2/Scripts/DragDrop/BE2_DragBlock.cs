using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.Environment;
using System.Linq;

namespace MG_BlocksEngine2.DragDrop
{
    public class BE2_DragBlock : MonoBehaviour, I_BE2_Drag
    {
        RectTransform _rectTransform;
        // v2.11 - references to drag drop manager and execution manager refactored in drag scripts
        BE2_DragDropManager _dragDropManager => BE2_DragDropManager.Instance;
        Transform _transform;
        public Transform Transform => _transform ? _transform : transform;
        BE2_Line _sourceLine;
        HolderEnvironment _sourceHolder;
        ScrollRect _programmingEnvScrollRect;

        // v2.13 ----> move raypoint to layout and create new variable to indicate the position of the ghost block when placed over, or a method  
        public Vector2 RayPoint => _rectTransform.position;

        public I_BE2_Block Block { get; set; }

        void Awake()
        {
            _transform = transform;
            _rectTransform = GetComponent<RectTransform>();
            Block = GetComponent<I_BE2_Block>();
        }

        public void OnPointerDown()
        {

        }

        public void OnRightPointerDownOrHold()
        {

        }

        // Line-based system: group dragging is not supported. Each block lives in its own line.
        public void OnDragStart()
        {
            _sourceLine = Transform.parent?.GetComponent<BE2_Line>();
            _sourceHolder = Transform.parent?.GetComponentInParent<HolderEnvironment>();
            // Fallback: if parent is the drag manager, look at the block itself for a holder
            if (_sourceHolder == null)
            {
                _sourceHolder = Transform.GetComponentInParent<HolderEnvironment>();
            }
            Debug.Log($"[BE2_DragBlock] OnDragStart: block={Transform.name}, parent={Transform.parent?.name}, _sourceLine={(_sourceLine != null ? _sourceLine.name : "NULL")}, _sourceHolder={(_sourceHolder != null ? _sourceHolder.name : "NULL")}");

            if (_sourceLine != null)
                _sourceLine.ClearBlock();
            else if (_sourceHolder != null)
                _sourceHolder.RemoveBlock(Block);
            else
                Debug.LogWarning($"[BE2_DragBlock] OnDragStart: block has NO sourceLine and NO sourceHolder! parent={Transform.parent?.name}");

            // Preserve the block's current scale when reparenting for drag
            Vector3 originalScale = Transform.localScale;
            if (Transform.parent != _dragDropManager.DraggedObjectsTransform)
                Transform.SetParent(_dragDropManager.DraggedObjectsTransform, true);
            Transform.localScale = originalScale;
            Debug.Log($"[BE2_DragBlock] OnDragStart: reparented to DraggedObjectsTransform");

            // Disable ProgrammingEnv scrolling while dragging a block
            if (_programmingEnvScrollRect == null)
            {
                I_BE2_ProgrammingEnv programmingEnv = Transform.GetComponentInParent<I_BE2_ProgrammingEnv>();
                if (programmingEnv != null)
                {
                    BE2_ProgrammingEnv env = programmingEnv as BE2_ProgrammingEnv;
                    if (env != null)
                        _programmingEnvScrollRect = env.scrollRect;
                }
            }
            if (_programmingEnvScrollRect == null)
                _programmingEnvScrollRect = Transform.GetComponentInParent<ScrollRect>();

            if (_programmingEnvScrollRect != null)
            {
                Debug.Log($"[BE2_DragBlock] OnDragStart: disabled ScrollRect {_programmingEnvScrollRect.name}");
                _programmingEnvScrollRect.StopMovement();
                _programmingEnvScrollRect.enabled = false;
            }
        }

        public void OnDrag()
        {
            DetectSpot();
        }

        void DetectSpot()
        {
            Vector3 originalScale = Transform.localScale;
            if (Transform.parent != _dragDropManager.DraggedObjectsTransform)
                Transform.SetParent(_dragDropManager.DraggedObjectsTransform, true);
            Transform.localScale = originalScale;

            if (_dragDropManager.Raycaster == null)
            {
                Debug.LogError("[BE2_DragBlock] DetectSpot: _dragDropManager.Raycaster is NULL!");
                return;
            }

            BE2_Raycaster raycaster = _dragDropManager.Raycaster as BE2_Raycaster;
            BE2_Line targetLine = raycaster.FindClosestEmptyLine(this, _dragDropManager.detectionDistance);
            HolderEnvironment targetHolder = raycaster.FindHolderEnvironmentAtPoint(RayPoint);
            Transform ghostBlockTransform = _dragDropManager.GhostBlockTransform;

            Debug.Log($"[BE2_DragBlock] DetectSpot: targetLine={(targetLine != null ? targetLine.name : "NULL")}, targetHolder={(targetHolder != null ? targetHolder.name : "NULL")}, CanPlace={(targetHolder != null ? targetHolder.CanPlaceBlock(Block) : false)}");

            ClearLineHighlights();
            ClearHolderHighlights();

            if (targetLine != null)
            {
                ghostBlockTransform.SetParent(targetLine.Transform);
                ghostBlockTransform.localPosition = Vector3.zero;
                ghostBlockTransform.gameObject.SetActive(true);
                targetLine.SetHover(true);
            }
            else if (targetHolder != null && targetHolder.CanPlaceBlock(Block))
            {
                ghostBlockTransform.SetParent(targetHolder.contentArea);
                Vector2 localPos = targetHolder.contentArea.InverseTransformPoint(RayPoint);
                ghostBlockTransform.localPosition = new Vector3(localPos.x, localPos.y, 0);
                ghostBlockTransform.gameObject.SetActive(true);
                targetHolder.SetHoverVisual(true);
            }
            else
            {
                ghostBlockTransform.gameObject.SetActive(false);
            }

            ghostBlockTransform.localPosition = new Vector3(ghostBlockTransform.localPosition.x, ghostBlockTransform.localPosition.y, 0);
            ghostBlockTransform.localEulerAngles = Vector3.zero;
        }

        void ClearLineHighlights()
        {
            foreach (I_BE2_Spot spot in _dragDropManager.SpotsList)
            {
                if (spot is BE2_Line line)
                    line.SetHover(false);
            }
        }

        void ClearHolderHighlights()
        {
            foreach (HolderEnvironment holder in HolderEnvironment.ActiveHolders)
                holder.SetHoverVisual(false);
        }

        // Line-based system: blocks drop into Lines
        public void OnPointerUp()
        {
            if (_dragDropManager.Raycaster == null)
            {
                Debug.LogError("[BE2_DragBlock] OnPointerUp: _dragDropManager.Raycaster is NULL!");
                return;
            }

            BE2_Raycaster raycaster = _dragDropManager.Raycaster as BE2_Raycaster;
            BE2_Line targetLine = raycaster.FindClosestEmptyLine(this, _dragDropManager.detectionDistance);
            HolderEnvironment targetHolder = raycaster.FindHolderEnvironmentAtPoint(RayPoint);

            Debug.Log($"[BE2_DragBlock] OnPointerUp: targetLine={(targetLine != null ? targetLine.name : "NULL")}, targetHolder={(targetHolder != null ? targetHolder.name : "NULL")}, _sourceHolder={(_sourceHolder != null ? _sourceHolder.name : "NULL")}");

            if (targetLine != null && !targetLine.IsOccupied)
            {
                Debug.Log($"[BE2_DragBlock] OnPointerUp: dropping to line {targetLine.name}");
                targetLine.SetBlock(Block);

                if (Block.Type == BlockTypeEnum.trigger)
                {
                    I_BE2_ProgrammingEnv programmingEnv = Transform.GetComponentInParent<I_BE2_ProgrammingEnv>();
                    if (programmingEnv != null)
                    {
                        BE2_ExecutionManager.Instance.AddToBlocksStackArray(Block.Instruction.InstructionBase.BlocksStack, programmingEnv.TargetObject);
                    }
                }
            }
            else if (targetHolder != null && targetHolder.CanPlaceBlock(Block))
            {
                Vector2 localPos = targetHolder.contentArea.InverseTransformPoint(RayPoint);
                Debug.Log($"[BE2_DragBlock] OnPointerUp: dropping to holder {targetHolder.name} at localPos={localPos}");
                targetHolder.AddBlock(Block, localPos);
            }
            else
            {
                // Final fallback: if we still have no source holder, try to find one from the block's current hierarchy
                if (_sourceHolder == null)
                {
                    _sourceHolder = Transform.GetComponentInParent<HolderEnvironment>();
                    Debug.Log($"[BE2_DragBlock] OnPointerUp: fallback sourceHolder search result={(_sourceHolder != null ? _sourceHolder.name : "NULL")}");
                }

                if (_sourceLine != null)
                {
                    Debug.Log($"[BE2_DragBlock] OnPointerUp: returning to source line {_sourceLine.name}");
                    _sourceLine.SetBlock(Block);
                }
                else if (_sourceHolder != null)
                {
                    Vector2 returnPos = _sourceHolder.contentArea.InverseTransformPoint(RayPoint);
                    Debug.Log($"[BE2_DragBlock] OnPointerUp: returning to source holder {_sourceHolder.name} at localPos={returnPos}");
                    _sourceHolder.AddBlock(Block, returnPos);
                }
                else
                {
                    StorageEnvironment nearestStorage = FindNearestStorageEnvironment(RayPoint);
                    if (nearestStorage != null)
                    {
                        Vector2 nearestPoint = GetNearestPointOnRect(nearestStorage.contentArea, RayPoint);
                        Vector2 localPos = nearestStorage.contentArea.InverseTransformPoint(nearestPoint);
                        Debug.Log($"[BE2_DragBlock] OnPointerUp: no source, dropping to nearest storage {nearestStorage.name} at localPos={localPos}");
                        nearestStorage.AddBlock(Block, localPos);
                    }
                    else
                    {
                        Debug.LogWarning("[BE2_DragBlock] OnPointerUp: NO sourceLine, NO sourceHolder, NO target, NO storage — DESTROYING block!");
                        Destroy(Transform.gameObject);
                    }
                }
            }

            _sourceLine = null;
            _sourceHolder = null;
            ClearLineHighlights();
            ClearHolderHighlights();
            _dragDropManager.GhostBlockTransform.gameObject.SetActive(false);

            Transform.localPosition = new Vector3(Transform.localPosition.x, Transform.localPosition.y, 0);
            Transform.localEulerAngles = Vector3.zero;

            // Re-enable ProgrammingEnv scrolling after drag ends
            if (_programmingEnvScrollRect != null)
            {
                _programmingEnvScrollRect.enabled = true;
                _programmingEnvScrollRect = null;
            }

            Block.Instruction.InstructionBase.UpdateTargetObject();
        }

        /// <summary>
        /// Finds the nearest StorageEnvironment to the given world point.
        /// Used as a fallback when an orphan block (no source line or holder) is dropped outside any environment.
        /// </summary>
        StorageEnvironment FindNearestStorageEnvironment(Vector2 worldPoint)
        {
            StorageEnvironment nearest = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (var holder in HolderEnvironment.ActiveHolders)
            {
                if (holder is not StorageEnvironment storage)
                    continue;

                if (storage.contentArea == null)
                    continue;

                if (!storage.CanPlaceBlock(Block))
                    continue;

                Vector2 nearestPoint = GetNearestPointOnRect(storage.contentArea, worldPoint);
                float distanceSqr = ((Vector2)nearestPoint - worldPoint).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearest = storage;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Returns the closest point on the RectTransform's world-space rectangle to the given point.
        /// </summary>
        Vector2 GetNearestPointOnRect(RectTransform rectTransform, Vector2 worldPoint)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            return new Vector2(
                Mathf.Clamp(worldPoint.x, minX, maxX),
                Mathf.Clamp(worldPoint.y, minY, maxY)
            );
        }
    }
}
