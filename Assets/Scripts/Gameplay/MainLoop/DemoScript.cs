using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    public InventoryScript InventoryScript;
    public BlockScript[] blocksToPickUp;

    public void PickUpItem(int id)
    {
        bool result = InventoryScript.AddBlock(blocksToPickUp[id]);
        if (result)
        {
            Debug.Log("ItemAdded");
        }
        else
        {
            Debug.Log("ItemNotAdded");
        }
    }
}

