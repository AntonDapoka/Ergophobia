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

            if (_sourceLine != null)
                _sourceLine.ClearBlock();
            else if (_sourceHolder != null)
                _sourceHolder.RemoveBlock(Block);

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
            if (Transform.parent != _dragDropManager.DraggedObjectsTransform)
                Transform.SetParent(_dragDropManager.DraggedObjectsTransform, true);

            BE2_Raycaster raycaster = _dragDropManager.Raycaster as BE2_Raycaster;
            BE2_Line targetLine = raycaster.FindClosestEmptyLine(this, _dragDropManager.detectionDistance);
            HolderEnvironment targetHolder = raycaster.FindHolderEnvironmentAtPoint(RayPoint);
            Transform ghostBlockTransform = _dragDropManager.GhostBlockTransform;

            ClearLineHighlights();
            ClearHolderHighlights();

            if (targetLine != null)
            {
                ghostBlockTransform.SetParent(targetLine.Transform);
                ghostBlockTransform.localPosition = Vector3.zero;
                ghostBlockTransform.localScale = Vector3.one;
                ghostBlockTransform.gameObject.SetActive(true);
                targetLine.SetHover(true);
            }
            else if (targetHolder != null && targetHolder.CanPlaceBlock(Block))
            {
                ghostBlockTransform.SetParent(targetHolder.contentArea);
                Vector2 localPos = targetHolder.contentArea.InverseTransformPoint(RayPoint);
                ghostBlockTransform.localPosition = new Vector3(localPos.x, localPos.y, 0);
                ghostBlockTransform.localScale = Vector3.one;
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
            BE2_Raycaster raycaster = _dragDropManager.Raycaster as BE2_Raycaster;
            BE2_Line targetLine = raycaster.FindClosestEmptyLine(this, _dragDropManager.detectionDistance);
            HolderEnvironment targetHolder = raycaster.FindHolderEnvironmentAtPoint(RayPoint);

            if (targetLine != null && !targetLine.IsOccupied)
            {
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
                targetHolder.AddBlock(Block, localPos);
            }
            else
            {
                if (_sourceLine != null)
                {
                    _sourceLine.SetBlock(Block);
                }
                else if (_sourceHolder != null)
                {
                    Vector2 returnPos = _sourceHolder.contentArea.InverseTransformPoint(RayPoint);
                    _sourceHolder.AddBlock(Block, returnPos);
                }
                else
                {
                    Destroy(Transform.gameObject);
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


    }
}
