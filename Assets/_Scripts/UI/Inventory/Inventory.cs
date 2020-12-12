using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventoryRow[] rows;
    public GameObject storage;
    public Toolbar toolbar = default;

    private void Start()
    {
        foreach (InventoryRow row in rows)
        {
            ushort index = 1;
            foreach (UIItemSlot s in row.slots)
            {
                ItemStack stack = new ItemStack((ushort)Random.Range(1, 7), Random.Range(1, 65));
                ItemSlot slot = new ItemSlot(s, stack);
                index++;
            }
        }
        storage.SetActive(false);
    }
}