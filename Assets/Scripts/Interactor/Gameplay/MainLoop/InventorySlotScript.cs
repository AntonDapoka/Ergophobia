using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotScript : MonoBehaviour, IDropHandler
{
    [SerializeField] private InventoryScript inventory; // Ссылка на инвентарь, чтобы знать maxStackedBlocks

    public void OnDrop(PointerEventData eventData)
    {
        DraggableBlockScript droppedBlock = eventData.pointerDrag.GetComponent<DraggableBlockScript>();
        if (droppedBlock == null) return;

        // Если слот пустой – просто кладем
        if (transform.childCount == 0)
        {
            droppedBlock.parentAfterDrag = transform;
            return;
        }

        // Если слот непустой – проверяем, можем ли стакнуть
        DraggableBlockScript existingBlock = GetComponentInChildren<DraggableBlockScript>();
        if (existingBlock != null &&
            existingBlock.block == droppedBlock.block &&                // один и тот же тип
            existingBlock.block.isStackable &&                         // можно стакать
            existingBlock.count < inventory.MaxStackedBlocks)          // не переполнен
        {
            // Сколько ещё можно добавить
            int freeSpace = inventory.MaxStackedBlocks - existingBlock.count;

            // Сколько переносим
            int amountToMove = Mathf.Min(droppedBlock.count, freeSpace);

            // Увеличиваем количество в слоте
            existingBlock.count += amountToMove;
            existingBlock.RefreshCount();

            // Уменьшаем количество у переносимого блока
            droppedBlock.count -= amountToMove;

            if (droppedBlock.count <= 0)
            {
                // Если блок опустел – удаляем его
                Destroy(droppedBlock.gameObject);
            }
            else
            {
                // Если что-то осталось, блок возвращается на прежнее место
                droppedBlock.RefreshCount();
                //droppedBlock.parentAfterDrag = droppedBlock.parentAfterDrag;
            }
        }
        else
        {
            // Если стакнуть нельзя – возвращаем предмет на место
            //droppedBlock.parentAfterDrag = droppedBlock.parentAfterDrag;
        }
    }
}