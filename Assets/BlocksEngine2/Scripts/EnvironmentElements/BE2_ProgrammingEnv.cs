using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.DragDrop;
using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Core;

namespace MG_BlocksEngine2.Environment
{
    public class BE2_ProgrammingEnv : MonoBehaviour, I_BE2_ProgrammingEnv
    {
        Transform _transform;
        public Transform Transform => _transform ? _transform : transform;
        public List<I_BE2_Block> BlocksList { get; set; }
        public BE2_TargetObject targetObject;
        public I_BE2_TargetObject TargetObject => targetObject;

        // --- Line-based system: main lines ---
        [Header("Lines")]
        public int maxMainLines = 20;
        public float lineHeight = 60f;
        public List<BE2_Line> MainLines { get; private set; }

        // v2.4 - added property to set visibility of Programming Environment, facilitates the use of multiple individualy programmable Target Objects in the same scene 
        [SerializeField]
        bool _visible = true;
        public bool Visible 
        {
            get => _visible;
            set
            {
                _visible = value;

                // v2.6 - bugfix: fixed null exception on play scene without target objects and programming envs
                // v2.4.1 - bugfix: fixed Null Exception on opening the "Target Object and Programming Env" prefabs 
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

        CanvasGroup _parentCanvasGroup;
        GraphicRaycaster _parentGraphicRaycaster;

        void OnValidate()
        {
            _parentCanvasGroup = GetComponentInParent<CanvasGroup>();
            Visible = _visible;
        }

        void Awake()
        {
            // v2.11 - added null check to avoid error if ProgrammingEnv has no TargetObject set
            if (targetObject)
            {
                // v2.5 - sets the ProgrammingEnv reference on the TargetObject
                targetObject.ProgrammingEnv = this;
            }

            _transform = transform;
            CreateMainLines();
            UpdateBlocksList();

            _parentCanvasGroup = GetComponentInParent<CanvasGroup>();
            _parentGraphicRaycaster = _parentCanvasGroup.GetComponent<GraphicRaycaster>();
        }

        void Start()
        {
            BE2_DragDropManager.Instance.Raycaster.AddRaycaster(_parentGraphicRaycaster);
        }

        void OnEnable()
        {
            BE2_MainEventsManager.Instance.StartListening(BE2EventTypes.OnPrimaryKeyUpEnd, UpdateLinePositions);
        }

        void OnDisable()
        {
            BE2_MainEventsManager.Instance.StopListening(BE2EventTypes.OnPrimaryKeyUpEnd, UpdateLinePositions);
        }

        void LateUpdate()
        {
            UpdateLinePositions();
        }

        public void UpdateLinePositions()
        {
            if (MainLines == null) return;

            float currentY = 0;
            for (int i = 0; i < MainLines.Count; i++)
            {
                BE2_Line line = MainLines[i];
                if (line == null) continue;

                RectTransform rt = line.GetComponent<RectTransform>();
                if (rt == null) continue;

                rt.anchoredPosition = new Vector2(0, currentY);

                if (line.CurrentBlock != null && line.CurrentBlock.Layout != null)
                {
                    currentY -= line.CurrentBlock.Layout.Size.y;
                }
                else
                {
                    currentY -= lineHeight;
                }
            }
        }

        void CreateMainLines()
        {
            MainLines = new List<BE2_Line>();
            for (int i = 0; i < maxMainLines; i++)
            {
                GameObject lineGO = new GameObject("Line " + i, typeof(RectTransform), typeof(Image), typeof(BE2_Line));
                lineGO.transform.SetParent(_transform);
                lineGO.transform.SetAsLastSibling();

                RectTransform rt = lineGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(0, -i * lineHeight);
                rt.sizeDelta = new Vector2(0, lineHeight);

                BE2_Line line = lineGO.GetComponent<BE2_Line>();
                line.LineIndex = i;
                MainLines.Add(line);
            }
        }

        public void UpdateBlocksList()
        {
            BlocksList = new List<I_BE2_Block>();
            if (MainLines != null)
            {
                foreach (BE2_Line line in MainLines)
                {
                    if (line != null && line.CurrentBlock != null)
                        BlocksList.Add(line.CurrentBlock);
                }
            }
        }

        public void ClearBlocks()
        {
            if (MainLines != null)
            {
                foreach (BE2_Line line in MainLines)
                {
                    if (line != null && line.CurrentBlock != null)
                    {
                        Destroy(line.CurrentBlock.Transform.gameObject);
                        line.ClearBlock();
                    }
                }
            }

            UpdateBlocksList();
        }
    }
}