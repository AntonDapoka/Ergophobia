using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block
{
    [ExecuteInEditMode]
    public class BE2_BlockSectionBody : MonoBehaviour, I_BE2_BlockSectionBody
    {
        I_BE2_BlockSection _section;
        I_BE2_BlockLayout _blockLayout;
        Image _image;
        RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;
        public I_BE2_Block[] ChildBlocksArray { get; set; }
        public I_BE2_BlockSection BlockSection { get; set; }
        public Vector2 Size
        {
            get
            {
                return _rectTransform.sizeDelta;
            }
            set
            {
                _rectTransform.sizeDelta = value;
            }
        }
        public I_BE2_Spot Spot { get; set; }
        public int ChildBlocksCount { get; set; }

        // --- Line-based system: sub-lines for nested blocks ---
        [Header("Sub Lines")]
        public int maxSubLines = 5;
        public float subLineHeight = 55f;
        public float subLineIndent = 20f;
        public List<BE2_Line> SubLines { get; private set; }
        Shadow _shadow;
        public Shadow Shadow
        {
            get
            {
                if (!_shadow)
                {
                    if (GetComponent<Shadow>())
                        _shadow = GetComponent<Shadow>();
                    else
                        _shadow = gameObject.AddComponent<Shadow>();

                    _shadow.effectColor = Color.green;
                    _shadow.effectDistance = new Vector2(-6, -6);
                }

                return _shadow;
            }
        }

        void OnValidate()
        {
            Awake();
        }

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (transform.parent)
            {
                _section = transform.parent.GetComponent<I_BE2_BlockSection>();
                _blockLayout = transform.parent.parent.GetComponent<I_BE2_BlockLayout>();
                BlockSection = transform.parent.GetComponent<I_BE2_BlockSection>();
            }

            _image = GetComponent<Image>();
            _image.type = Image.Type.Sliced;
            _image.pixelsPerUnitMultiplier = 2;

            ChildBlocksArray = new I_BE2_Block[0];
            Spot = GetComponent<I_BE2_Spot>();

            CreateSubLines();
        }

        void CreateSubLines()
        {
            if (!Application.isPlaying) return;
            if (_section == null) return;

            BlockTypeEnum type = _section.Block.Type;
            if (type != BlockTypeEnum.condition && type != BlockTypeEnum.loop && type != BlockTypeEnum.define)
                return;

            SubLines = new List<BE2_Line>();
            for (int i = 0; i < maxSubLines; i++)
            {
                GameObject lineGO = new GameObject("SubLine " + i, typeof(RectTransform), typeof(Image), typeof(BE2_Line));
                lineGO.transform.SetParent(transform);
                lineGO.transform.SetAsLastSibling();

                RectTransform rt = lineGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(subLineIndent, -i * subLineHeight);
                rt.sizeDelta = new Vector2(-subLineIndent * 2, subLineHeight);

                BE2_Line line = lineGO.GetComponent<BE2_Line>();
                line.LineIndex = i;
                line.ParentBody = this;
                SubLines.Add(line);
            }
        }

        //void Start()
        //{
        //
        //}

        //void Update()
        //{
        //    //UpdateLayout();
        //}

        public void UpdateChildBlocksList()
        {
            ChildBlocksArray = new I_BE2_Block[0];

            if (SubLines != null && SubLines.Count > 0)
            {
                foreach (BE2_Line line in SubLines)
                {
                    if (line != null && line.CurrentBlock != null)
                    {
                        ChildBlocksArray = BE2_ArrayUtils.AddReturn(ChildBlocksArray, line.CurrentBlock);
                    }
                }
            }
            else
            {
                // Fallback for backwards compatibility
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    I_BE2_Block childBlock = transform.GetChild(i).GetComponent<I_BE2_Block>();
                    if (childBlock != null)
                    {
                        ChildBlocksArray = BE2_ArrayUtils.AddReturn(ChildBlocksArray, childBlock);
                    }
                }
            }

            ChildBlocksCount = ChildBlocksArray.Length;
        }

        public void UpdateLayout()
        {
            if (_image.sprite != null && _blockLayout != null)
                _image.color = _blockLayout.Color;

            float height = 0;

            if (SubLines != null && SubLines.Count > 0)
            {
                height = SubLines.Count * subLineHeight;
            }
            else
            {
                float minHeight = 50;
                if (_section.Block.Type == BlockTypeEnum.trigger || _section.Block.Type == BlockTypeEnum.define)
                    minHeight = 0;

                UpdateChildBlocksList();
                int childsLength = ChildBlocksArray.Length;
                for (int i = 0; i < childsLength; i++)
                {
                    height += ChildBlocksArray[i].Layout.Size.y - 10;
                }
                height -= 10;

                if (height < minHeight)
                    height = minHeight;

                if (_section.RectTransform.transform.GetSiblingIndex() == _section.RectTransform.transform.parent.childCount - 2)
                {
                    if (_section.Block.Type != BlockTypeEnum.trigger && _section.Block.Type != BlockTypeEnum.define)
                    {
                        height += 50;
                    }
                }
            }

            _rectTransform.sizeDelta = new Vector2(_section.Size.x, height);
        }
    }
}