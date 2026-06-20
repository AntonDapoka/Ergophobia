using UnityEngine;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Environment;

namespace MG_BlocksEngine2.Block.Instruction
{
    public class BE2_Ins_SpacecraftShoot : BE2_InstructionBase, I_BE2_Instruction
    {
        public override void Function()
        {
            GameObject projectilePrefab = GetProjectilePrefab();

            if (projectilePrefab != null && TargetObject is BE2_TargetObjectSpacecraft3D spacecraft)
            {
                spacecraft.ShootBullet(projectilePrefab);
            }

            ExecuteNextInstruction();
        }

        /// <summary>
        /// Tries to retrieve the prefab from the first input slot.
        /// Only accepts blocks of type BE2_Op_Prefab; any other block type is ignored safely.
        /// </summary>
        GameObject GetProjectilePrefab()
        {
            if (Section0Inputs == null || Section0Inputs.Length == 0)
                return null;

            I_BE2_BlockSectionHeaderInput input = Section0Inputs[0];
            if (input == null || input.Transform == null)
                return null;

            BE2_Op_Prefab prefabBlock = input.Transform.GetComponent<BE2_Op_Prefab>();
            if (prefabBlock == null)
                return null;

            return prefabBlock.Prefab;
        }
    }
}
