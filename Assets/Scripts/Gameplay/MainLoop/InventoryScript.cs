using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    [SerializeField] private int maxStackedBlocks = 5;
    [SerializeField] private InventorySlotScript[] inventorySlots;
    [SerializeField] private GameObject draggableBlockPrefab;

    public bool AddBlock(BlockScript block)
    {

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlotScript slot = inventorySlots[i];
            DraggableBlockScript blockInSlot = slot.GetComponentInChildren<DraggableBlockScript>();
            if (blockInSlot != null &&
                blockInSlot.block == block &&
                blockInSlot.count < maxStackedBlocks &&
                blockInSlot.block.isStackable)

            {
                blockInSlot.count++;
                blockInSlot.RefreshCount();
                return true;
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlotScript slot = inventorySlots[i];
            DraggableBlockScript blockInSlot = slot.GetComponentInChildren<DraggableBlockScript>();
            if (blockInSlot == null)
            {
                SpawnBlock(block, slot);
                return true;
            }
        }
        return false;
    }

    public void SpawnBlock(BlockScript block, InventorySlotScript slot)
    {
        GameObject newBlockGo = Instantiate(draggableBlockPrefab, slot.transform);
        DraggableBlockScript draggableBlock = newBlockGo.GetComponent<DraggableBlockScript>();
        draggableBlock.InitializeBlock(block);


    }

}
