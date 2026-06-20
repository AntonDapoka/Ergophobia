using UnityEngine;

using CodeblockEntities;
using MG_BlocksEngine2.Utils;

namespace MG_BlocksEngine2.Block.Instruction
{
    public abstract class BE2_Op_EntityBase : BE2_InstructionBase, I_BE2_Instruction
    {
        [SerializeField] private Transform _nameLabelTransform;

        BE2_Text _nameLabel;

        public abstract CodeblockEntityType EntityType { get; }

        void OnValidate() => UpdateLabel();
        void Start() => UpdateLabel();

        public override string Operation()
        {
            return EntityType.ToString();
        }

        void UpdateLabel()
        {
            if (_nameLabelTransform == null) return;
            _nameLabel ??= BE2_Text.GetBE2Text(_nameLabelTransform);

            if (_nameLabel != null && !_nameLabel.isNull)
                _nameLabel.text = EntityType.ToString();
        }
    }
}
