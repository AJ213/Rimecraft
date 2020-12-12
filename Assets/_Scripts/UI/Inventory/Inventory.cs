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
            foreach (UIItemSlot s in row.slots)
            {
                //ItemStack stack = new ItemStack((ushort)Random.Range(1, 7), Random.Range(1, 65));
                ItemSlot slot = new ItemSlot(s);
            }
        }
        storage.SetActive(false);
    }

    public bool TryAdd(ref ItemStack items)
    {
        // Is there a slot with the same id as us and is open
        for (int i = rows.Length - 1; i >= 0; i--)
        {
            for (int j = 0; j < rows[i].slots.Length; j++)
            {
                if (rows[i].slots[j].itemSlot.stack == null)
                {
                    continue;
                }
                if (rows[i].slots[j].itemSlot.stack.id != items.id)
                {
                    continue;
                }
                if (rows[i].slots[j].itemSlot.stack.amount >= ItemStack.maxSize)
                {
                    continue;
                }

                if (rows[i].slots[j].itemSlot.stack.amount + items.amount < ItemStack.maxSize)
                {
                    ItemStack resultingStack = new ItemStack(rows[i].slots[j].itemSlot.stack.id, rows[i].slots[j].itemSlot.stack.amount + items.amount);
                    rows[i].slots[j].itemSlot.InsertStack(resultingStack);
                    return true;
                }
                else
                {
                    int takeAmount = ItemStack.maxSize - rows[i].slots[j].itemSlot.stack.amount;
                    ItemStack resultingStack = new ItemStack(items.id, ItemStack.maxSize);
                    rows[i].slots[j].itemSlot.InsertStack(resultingStack);
                    items.amount -= takeAmount;
                }
            }
        }

        // Is there an open slot?
        for (int i = rows.Length - 1; i >= 0; i--)
        {
            for (int j = 0; j < rows[i].slots.Length; j++)
            {
                if (rows[i].slots[j].itemSlot.stack != null)
                {
                    continue;
                }

                rows[i].slots[j].itemSlot.InsertStack(items);
                return true;
            }
        }
        return false;
    }
}