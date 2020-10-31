using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot
{
    public ItemStack stack;
    private UIItemSlot uiItemSlot = null;

    public ItemSlot(UIItemSlot itemSlot)
    {
        stack = null;
        uiItemSlot = itemSlot;
        uiItemSlot.Link(this);
    }

    public ItemSlot(UIItemSlot itemSlot, ItemStack itemStack)
    {
        stack = itemStack;
        uiItemSlot = itemSlot;
        uiItemSlot.Link(this);
    }

    public void LinkUISlot(UIItemSlot uiSlot)
    {
        uiItemSlot = uiSlot;
    }

    public void UnlinkUISlot()
    {
        uiItemSlot = null;
    }

    public void EmptySlot()
    {
        stack = null;
        if (uiItemSlot != null)
        {
            uiItemSlot.UpdateSlot();
        }
    }

    public int Take(int amt)
    {
        if (amt > stack.amount)
        {
            int amount = stack.amount;
            EmptySlot();
            return amount;
        }
        else if (amt < stack.amount)
        {
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertStack(ItemStack stack)
    {
        this.stack = stack;
        uiItemSlot.UpdateSlot();
    }

    public bool HasItem
    {
        get
        {
            return (stack != null);
        }
    }
}