using System.Globalization;
using UnityEngine;

using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block.Instruction
{
    /// <summary>
    /// Operation block that exposes a fixed positive integer value.
    /// The value is set in the prefab inspector and cannot be changed by the player at runtime.
    /// It can be plugged into condition blocks (Equal, BiggerThan, etc.) or loop blocks (Repeat).
    /// </summary>
    public class BE2_Op_Variable : BE2_InstructionBase, I_BE2_Instruction,
        I_BE2_BlockSectionHeaderItem, I_BE2_BlockSectionHeaderInput
    {
        [Header("Value")]
        [SerializeField]
        int _value = 1;

        [Header("UI Reference")]
        [SerializeField]
        Transform _valueLabelTransform;

        BE2_Text _valueLabel;

        public int Value => _value;

        // I_BE2_BlockSectionHeaderItem
        public Transform Transform => transform;
        public Vector2 Size => (transform as RectTransform)?.sizeDelta ?? Vector2.zero;

        // I_BE2_BlockSectionHeaderInput
        public I_BE2_Spot Spot => null;
        public float FloatValue => _value;
        public string StringValue => Operation();
        public BE2_InputValues InputValues => new BE2_InputValues(StringValue, FloatValue, false);

        public void UpdateValues()
        {
            // Value is fixed at runtime; nothing to update.
        }

        void OnValidate()
        {
            _value = Mathf.Max(0, _value);
            UpdateLabel();
        }

        void Start()
        {
            UpdateLabel();
        }

        public override string Operation()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }

        void UpdateLabel()
        {
            if (_valueLabelTransform == null)
                return;

            if (_valueLabel == null)
                _valueLabel = BE2_Text.GetBE2Text(_valueLabelTransform);

            if (_valueLabel != null && !_valueLabel.isNull)
                _valueLabel.text = _value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
