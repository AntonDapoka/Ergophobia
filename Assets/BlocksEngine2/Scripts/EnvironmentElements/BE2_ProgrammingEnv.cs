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

        [Header("Lines")]
        [Tooltip("Number of lines available when the programming environment starts.")]
        public int startMainLines = 4;
        [Tooltip("Maximum total lines the player can add to the programming environment.")]
        public int maxMainLines = 20;
        public float lineHeight = 60f;
        public float lineIndent = 20f;
        public float lineSpacing = 10f;
        public float lineWidth = 600f;
        public List<BE2_Line> MainLines { get; private set; }

        [Header("Line Appearance")]
        public Color lineNormalColor = new Color(0.15f, 0.15f, 0.15f, 0.3f);
        public Color lineHoverColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        public Color lineOccupiedColor = new Color(0.1f, 0.1f, 0.1f, 0.1f);
        public Sprite lineSprite;

        [Header("Scrolling")]
        public ScrollRect scrollRect;
        public RectTransform contentContainer;

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

            if (startMainLines > maxMainLines)
            {
                Debug.LogWarning($"[BE2_ProgrammingEnv] startMainLines ({startMainLines}) cannot exceed maxMainLines ({maxMainLines}). It will be clamped at runtime.");
            }
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
            SetupScrolling();
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
            float totalHeight = 0;
            float maxContentWidth = lineWidth;

            for (int i = 0; i < MainLines.Count; i++)
            {
                BE2_Line line = MainLines[i];
                if (line == null) continue;

                RectTransform rt = line.GetComponent<RectTransform>();
                if (rt == null) continue;

                rt.anchoredPosition = new Vector2(lineIndent, currentY);

                float lineSize = lineHeight;
                if (line.CurrentBlock != null && line.CurrentBlock.Layout != null)
                {
                    lineSize = Mathf.Max(lineHeight, line.CurrentBlock.Layout.Size.y);
                    float blockRightEdge = lineIndent + line.CurrentBlock.Layout.Size.x + lineIndent;
                    maxContentWidth = Mathf.Max(maxContentWidth, blockRightEdge);
                }

                currentY -= lineSize + lineSpacing;
                totalHeight += lineSize + lineSpacing;
            }

            if (contentContainer != null)
            {
                contentContainer.sizeDelta = new Vector2(maxContentWidth, totalHeight);
            }
        }

        void SetupScrolling()
        {
            // Ensure RectMask2D for clipping overflow
            if (GetComponent<RectMask2D>() == null)
                gameObject.AddComponent<RectMask2D>();

            // Ensure ScrollRect
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>() ?? gameObject.AddComponent<ScrollRect>();

            // Ensure content container
            if (contentContainer == null)
            {
                Transform existingContent = transform.Find("Content");
                if (existingContent != null)
                {
                    contentContainer = existingContent.GetComponent<RectTransform>();
                }
                else
                {
                    GameObject contentGO = new GameObject("Content", typeof(RectTransform));
                    contentContainer = contentGO.GetComponent<RectTransform>();
                    contentContainer.SetParent(transform, false);
                    contentContainer.anchorMin = new Vector2(0, 1);
                    contentContainer.anchorMax = new Vector2(0, 1);
                    contentContainer.pivot = new Vector2(0, 1);
                    contentContainer.anchoredPosition = Vector2.zero;
                    contentContainer.sizeDelta = new Vector2(lineWidth, 0);
                }
            }
            scrollRect.content = contentContainer;
            scrollRect.viewport = GetComponent<RectTransform>();
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.horizontalScrollbar = EnsureHorizontalScrollbar();
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        Scrollbar EnsureHorizontalScrollbar()
        {
            Transform existing = transform.Find("HorizontalScrollbar");
            if (existing != null && existing.TryGetComponent<Scrollbar>(out var existingBar))
                return existingBar;

            GameObject barGO = new GameObject("HorizontalScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            barGO.transform.SetParent(transform, false);

            RectTransform barRT = barGO.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(1, 0);
            barRT.pivot = new Vector2(0, 0);
            barRT.anchoredPosition = Vector2.zero;
            barRT.sizeDelta = new Vector2(0, 20);

            Image bg = barGO.GetComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Scrollbar scrollbar = barGO.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.LeftToRight;

            GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(barGO.transform, false);
            RectTransform slidingRT = slidingArea.GetComponent<RectTransform>();
            slidingRT.anchorMin = Vector2.zero;
            slidingRT.anchorMax = Vector2.one;
            slidingRT.pivot = new Vector2(0.5f, 0.5f);
            slidingRT.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(slidingArea.transform, false);
            RectTransform handleRT = handle.GetComponent<RectTransform>();
            handleRT.anchorMin = Vector2.zero;
            handleRT.anchorMax = Vector2.one;
            handleRT.pivot = new Vector2(0.5f, 0.5f);
            handleRT.sizeDelta = new Vector2(-20, -20);

            Image handleImg = handle.GetComponent<Image>();
            handleImg.color = new Color(0.4f, 0.4f, 0.4f, 0.9f);

            scrollbar.handleRect = handleRT;
            scrollbar.targetGraphic = handleImg;
            scrollbar.size = 0.1f;

            return scrollbar;
        }

        void CreateMainLines()
        {
            MainLines = new List<BE2_Line>();
            int initialLines = Mathf.Min(startMainLines, maxMainLines);
            for (int i = 0; i < initialLines; i++)
            {
                GameObject lineGO = new GameObject("Line " + i, typeof(RectTransform), typeof(Image), typeof(BE2_Line));
                lineGO.transform.SetParent(contentContainer != null ? contentContainer : _transform);
                lineGO.transform.SetAsLastSibling();
                lineGO.transform.localScale = Vector3.one;

                RectTransform rt = lineGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(lineIndent, -i * lineHeight);
                rt.sizeDelta = new Vector2(lineWidth, lineHeight);
                Image img = lineGO.GetComponent<Image>();
                
                if (lineSprite != null)
                    img.sprite = lineSprite;

                BE2_Line line = lineGO.GetComponent<BE2_Line>();
                line.LineIndex = i;
                line.NormalColor = lineNormalColor;
                line.HoverColor = lineHoverColor;
                line.OccupiedColor = lineOccupiedColor;
                line.BackgroundSprite = lineSprite;
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

        /// <summary>
        /// Adds a single empty main line at the bottom of the programming environment.
        /// Does nothing if the configured maximum number of lines has been reached.
        /// </summary>
        public void AddLine()
        {
            if (MainLines != null && MainLines.Count >= maxMainLines) return;

            if (MainLines == null) MainLines = new List<BE2_Line>();

            if (contentContainer == null) SetupScrolling();

            int index = MainLines.Count;
            GameObject lineGO = new GameObject("Line " + index, typeof(RectTransform), typeof(Image), typeof(BE2_Line));
            lineGO.transform.SetParent(contentContainer != null ? contentContainer : _transform);
            lineGO.transform.SetAsLastSibling();
            lineGO.transform.localScale = Vector3.one;

            RectTransform rt = lineGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(lineIndent, -index * lineHeight);
            rt.sizeDelta = new Vector2(lineWidth, lineHeight);

            Image img = lineGO.GetComponent<Image>();
            if (lineSprite != null)
                img.sprite = lineSprite;

            BE2_Line line = lineGO.GetComponent<BE2_Line>();
            line.LineIndex = index;
            line.NormalColor = lineNormalColor;
            line.HoverColor = lineHoverColor;
            line.OccupiedColor = lineOccupiedColor;
            line.BackgroundSprite = lineSprite;

            MainLines.Add(line);

            UpdateBlocksList();
            UpdateLinePositions();
        }
    }
}