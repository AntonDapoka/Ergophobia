using UnityEngine;

using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block.Instruction
{
    public class BE2_Op_Prefab : BE2_InstructionBase, I_BE2_Instruction
    {
        [Header("Prefab")]
        [SerializeField]
        [Tooltip("The prefab this block represents.")]
        GameObject _prefab;

        [Header("UI Reference")]
        [SerializeField]
        Transform _nameLabelTransform;

        BE2_Text _nameLabel;

        public GameObject Prefab => _prefab;

        void OnValidate()
        {
            UpdateLabel();
        }

        void Start()
        {
            UpdateLabel();
        }

        public override string Operation()
        {
            return _prefab != null ? _prefab.name : "0";
        }

        void UpdateLabel()
        {
            if (_nameLabelTransform == null)
                return;

            if (_nameLabel == null)
                _nameLabel = BE2_Text.GetBE2Text(_nameLabelTransform);

            if (_nameLabel != null && !_nameLabel.isNull)
                _nameLabel.text = _prefab != null ? _prefab.name : "None";
        }
    }
}
