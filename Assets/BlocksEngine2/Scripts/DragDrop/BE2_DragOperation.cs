using UnityEngine;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Environment;

namespace MG_BlocksEngine2.DragDrop
{
    public class BE2_DragOperation : MonoBehaviour, I_BE2_Drag
    {
        // v2.11 - references to drag drop manager and execution manager refactored in drag scripts
        BE2_DragDropManager _dragDropManager => BE2_DragDropManager.Instance;
        RectTransform _rectTransform;
        // v2.10.2 - bugfix: blocks that start inside the programming env with an operation block as input get no input spot back after dragging operation block out
        [HideInInspector] [SerializeField] Transform _usedSpotTransform;  // former I_BE2_Spot _usedSpot

        Transform _transform;
        public Transform Transform => _transform ? _transform : transform;
        public Vector2 RayPoint => _rectTransform.position;
        public I_BE2_Block Block { get; set; }

        void Awake()
        {
            _transform = transform;
            _rectTransform = GetComponent<RectTransform>();
            Block = GetComponent<I_BE2_Block>();
        }

        public void OnPointerDown() {}

        public void OnRightPointerDownOrHold() {}

        public void OnDragStart() {}

        public void OnDrag()
        {
            if (_usedSpotTransform != null)
            {
                _usedSpotTransform.SetSiblingIndex(Transform.GetSiblingIndex());
                _usedSpotTransform.gameObject.SetActive(true);
                _usedSpotTransform = null;
            }

            Vector3 originalScale = Transform.localScale;
            if (Transform.parent != _dragDropManager.DraggedObjectsTransform)
                Transform.SetParent(_dragDropManager.DraggedObjectsTransform, true);
            Transform.localScale = originalScale;

            BE2_Raycaster.ConnectionPoint connectionPoint = new BE2_Raycaster.ConnectionPoint();
            I_BE2_Spot spot = _dragDropManager.Raycaster.FindClosestSpotOfType<BE2_SpotBlockInput>(this, _dragDropManager.detectionDistance);

            if (spot != null)
            {
                if (_dragDropManager.ConnectionPoint.spot != null && _dragDropManager.ConnectionPoint.spot != spot)
                    (_dragDropManager.ConnectionPoint.spot as BE2_SpotBlockInput).outline.enabled = false;

                connectionPoint.spot = spot;
                _dragDropManager.ConnectionPoint = connectionPoint;
                (_dragDropManager.ConnectionPoint.spot as BE2_SpotBlockInput).outline.enabled = true;
            }
            else
            {
                if (_dragDropManager.ConnectionPoint.spot != null)
                {
                    (_dragDropManager.ConnectionPoint.spot as BE2_SpotBlockInput).outline.enabled = false;
                    connectionPoint.spot = null;
                    _dragDropManager.ConnectionPoint = connectionPoint;
                }
            }
        }

        public void OnPointerUp()
        {
            bool dropped = false;

            if (_dragDropManager.ConnectionPoint.spot != null)
            {
                I_BE2_Spot spot = _dragDropManager.ConnectionPoint.spot;

                // v2.x - support optional spot filters to restrict which block types can be dropped
                if (spot is I_BE2_SpotFilter filter && !filter.CanAcceptBlock(Block))
                {
                    if (spot is BE2_SpotBlockInput inputSpot)
                        inputSpot.outline.enabled = false;

                    _dragDropManager.ConnectionPoint = new BE2_Raycaster.ConnectionPoint();
                }
                else
                {
                    DropTo(spot);
                    dropped = true;
                }
            }

            if (!dropped)
            {
                // v2.x - allow operation blocks to be dropped into holder environments (inventory/storage)
                BE2_Raycaster raycaster = _dragDropManager.Raycaster as BE2_Raycaster;
                HolderEnvironment targetHolder = raycaster?.FindHolderEnvironmentAtPoint(RayPoint);
                if (targetHolder != null && targetHolder.CanPlaceBlock(Block))
                {
                    Vector2 localPos = targetHolder.contentArea.InverseTransformPoint(RayPoint);
                    targetHolder.AddBlock(Block, localPos);
                }
                else
                {
                    I_BE2_Spot spot = _dragDropManager.Raycaster.GetSpotAtPosition(RayPoint);

                    // v2.12 - dropping blocks in the ProgrammingEnv now can be done if part of the block is outside
                    // of the Env but the pointer is inside 
                    if (spot == null)
                        spot = _dragDropManager.Raycaster.GetSpotAtPosition(Core.BE2_InputManager.Instance.CanvasPointerPosition);

                    if (spot != null)
                    {
                        I_BE2_ProgrammingEnv programmingEnv = spot.Transform.GetComponentInParent<I_BE2_ProgrammingEnv>();
                        if (programmingEnv == null && spot.Transform.GetChild(0) != null)
                            programmingEnv = spot.Transform.GetChild(0).GetComponentInParent<I_BE2_ProgrammingEnv>();

                        Vector3 dropScale = Transform.localScale;
                        if (programmingEnv != null)
                            Transform.SetParent(programmingEnv.Transform);
                        else
                            Destroy(Transform.gameObject);
                        Transform.localScale = dropScale;
                    }
                    else
                    {
                        StorageEnvironment nearestStorage = FindNearestStorageEnvironment(RayPoint);
                        if (nearestStorage != null)
                        {
                            Vector2 nearestPoint = GetNearestPointOnRect(nearestStorage.contentArea, RayPoint);
                            Vector2 localPos = nearestStorage.contentArea.InverseTransformPoint(nearestPoint);
                            nearestStorage.AddBlock(Block, localPos);
                        }
                        else
                        {
                            Destroy(Transform.gameObject);
                        }
                    }
                }
            }

            // v2.6 - adjustments on position and angle of blocks for supporting all canvas render modes
            Transform.localPosition = new Vector3(Transform.localPosition.x, Transform.localPosition.y, 0);
            Transform.localEulerAngles = Vector3.zero;

            // v2.9 - bugfix: TargetObject of blocks being null
            Block.Instruction.InstructionBase.UpdateTargetObject();
        }

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

        // v2.1 - bugfix: fixed destroying operations placed as inputs causing error 
        void OnDisable()
        {
            if (_usedSpotTransform != null)
            {
                // v2.3 - bugfix: fixed intermittent error "cannot change sibling OnDisable"
                if (_usedSpotTransform.gameObject != null)
                    _usedSpotTransform.gameObject.SetActive(true);
                _usedSpotTransform = null;
            }

            // Guard against shutdown / scene unload ordering where the drag manager or transform
            // may already be destroyed when block prefabs are disabled.
            var manager = _dragDropManager;
            if (manager == null || manager.DraggedObjectsTransform == null)
                return;

            if (Transform == null || Transform.gameObject == null)
                return;

            if (Transform.parent != manager.DraggedObjectsTransform)
                Transform.gameObject.SetActive(false);
        }

        void DropTo(I_BE2_Spot spot)
        {
            Vector3 originalScale = Transform.localScale;
            Transform.SetParent(spot.Transform.parent);
            Transform.SetSiblingIndex(spot.Transform.GetSiblingIndex());
            Transform.localScale = originalScale;

            (spot as BE2_SpotBlockInput).outline.enabled = false;
            _usedSpotTransform = spot.Transform;
            _usedSpotTransform.gameObject.SetActive(false);
        }
    }
}
