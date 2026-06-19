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
    public class BE2_Op_Variable : BE2_InstructionBase, I_BE2_Instruction
    {
        [Header("Value")]
        [SerializeField]
        [Tooltip("Positive integer value exposed by this block.")]
        int _value = 1;

        [Header("UI Reference")]
        [SerializeField]
        [Tooltip("Optional child Transform containing a Text or TMP_Text component used to display the value.")]
        Transform _valueLabelTransform;

        BE2_Text _valueLabel;

        public int Value => _value;

        void OnValidate()
        {
            // Enforce the "positive integer" constraint
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
