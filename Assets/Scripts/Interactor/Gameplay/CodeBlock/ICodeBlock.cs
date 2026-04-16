using UnityEngine;

public interface ICodeBlock
    {
        public Transform Transform { get; }

        public BlockType Type { get; set; } // Defines the block internal execution characteritics

        public IBlockLayout Layout { get; }
        public IInstruction Instruction { get; set; }
        public IBlockSection ParentSection { get; set; }

        public ICodeBlock ParentBlock { get; set; }
        public IDrag Drag { get; }

        public void SetShadowActive(bool value); // Set visible/hidden the block hilight, used to identify a running/active block  
    }