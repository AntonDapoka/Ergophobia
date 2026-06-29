using System.Collections.Generic;
using UnityEngine;

using CodeblockEntities;
using MG_BlocksEngine2.Block;

namespace MG_BlocksEngine2.Block.Instruction
{
    /// <summary>
    /// Base class for instruction blocks that apply an effect to a group of entities.
    /// Expects Section0Inputs[0] to be an entity block (Enemy/Player/Bullet)
    /// and Section0Inputs[1] to be an optional numeric value.
    /// </summary>
    public abstract class BE2_Ins_EntityModifierBase : BE2_InstructionBase, I_BE2_Instruction
    {
        [Header("Default Value")]
        [SerializeField]
        [Tooltip("Value used when no numeric input block is connected.")]
        float _defaultValue = 1f;

        protected abstract void ApplyToEntity(ICodeblockEntity entity, float value);

        protected void ApplyModifier()
        {
            BE2_Op_EntityBase entityBlock = GetEntityBlock();
            if (entityBlock == null)
                return;

            ICodeblockEntityManager manager = CodeblockManagersHub.Instance?.GetManager(entityBlock.EntityType);
            if (manager == null)
                return;

            float value = GetValue();
            // Snapshot the entities so that killing an entity during modification
            // (which removes it from the manager's live list) does not invalidate
            // the enumeration.
            List<ICodeblockEntity> entitiesSnapshot = new List<ICodeblockEntity>(manager.GetEntities());
            foreach (ICodeblockEntity entity in entitiesSnapshot)
            {
                if (entity == null) continue;
                ApplyToEntity(entity, value);
            }
        }

        BE2_Op_EntityBase GetEntityBlock()
        {
            if (Section0Inputs == null || Section0Inputs.Length == 0)
                return null;

            I_BE2_BlockSectionHeaderInput input = Section0Inputs[0];
            if (input == null || input.Transform == null)
                return null;

            return input.Transform.GetComponent<BE2_Op_EntityBase>();
        }

        protected float GetValue()
        {
            if (Section0Inputs != null && Section0Inputs.Length > 1 && Section0Inputs[1] != null)
                return Section0Inputs[1].FloatValue;

            return _defaultValue;
        }
    }
}
