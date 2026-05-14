using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Core;
using MG_BlocksEngine2.Environment;

namespace MG_BlocksEngine2.DragDrop
{
    public class BE2_DragTrigger : MonoBehaviour, I_BE2_Drag
    {
        // v2.11 - references to drag drop manager and execution manager refactored in drag scripts
        BE2_DragDropManager _dragDropManager => BE2_DragDropManager.Instance;
        BE2_ExecutionManager _executionManager => BE2_ExecutionManager.Instance;
        RectTransform _rectTransform;
        // v2.12 - removed unused blocks stack variable from the DragTrigger class

        Transform _transform;
        public Transform Transform => _transform ? _transform : transform;
        BE2_Line _sourceLine;
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

        // Line-based system: group dragging is not supported.
        public void OnDragStart()
        {
            _sourceLine = Transform.parent?.GetComponent<BE2_Line>();
            _sourceLine?.ClearBlock();
        }

        public void OnDrag()
        {
            DetectSpot();
        }

        void DetectSpot()
        {
            if (Transform.parent != _dragDropManager.DraggedObjectsTransform)
                Transform.SetParent(_dragDropManager.DraggedObjectsTransform, true);

            BE2_Line targetLine = (_dragDropManager.Raycaster as BE2_Raycaster).FindClosestEmptyLine(this, _dragDropManager.detectionDistance);
            Transform ghostBlockTransform = _dragDropManager.GhostBlockTransform;

            ClearLineHighlights();

            if (targetLine != null)
            {
                ghostBlockTransform.SetParent(targetLine.Transform);
                ghostBlockTransform.localPosition = Vector3.zero;
                ghostBlockTransform.localScale = Vector3.one;
                ghostBlockTransform.gameObject.SetActive(true);
                targetLine.SetHover(true);
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

        // Line-based system: blocks drop into Lines
        public void OnPointerUp()
        {
            BE2_Line targetLine = (_dragDropManager.Raycaster as BE2_Raycaster).FindClosestEmptyLine(this, _dragDropManager.detectionDistance);

            if (targetLine != null && !targetLine.IsOccupied)
            {
                targetLine.SetBlock(Block);

                I_BE2_ProgrammingEnv programmingEnv = Transform.GetComponentInParent<I_BE2_ProgrammingEnv>();
                if (programmingEnv != null)
                {
                    _executionManager.AddToBlocksStackArray(Block.Instruction.InstructionBase.BlocksStack, programmingEnv.TargetObject);
                }
            }
            else
            {
                if (_sourceLine != null)
                {
                    _sourceLine.SetBlock(Block);
                }
                else
                {
                    Destroy(Transform.gameObject);
                }
            }

            _sourceLine = null;
            ClearLineHighlights();
            _dragDropManager.GhostBlockTransform.gameObject.SetActive(false);

            Transform.localPosition = new Vector3(Transform.localPosition.x, Transform.localPosition.y, 0);
            Transform.localEulerAngles = Vector3.zero;

            Block.Instruction.InstructionBase.UpdateTargetObject();
        }
    }
}
