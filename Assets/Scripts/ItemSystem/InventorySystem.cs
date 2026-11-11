using System;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private int maxSlots = 20;
    private ItemInstance[] slots;
    public int MaxSlots => maxSlots;
    public ItemInstance[] Slots => slots;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        slots = new ItemInstance[maxSlots];
    }

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        // 既存スロットへのスタック処理
        if (itemData.maxStackSize > 1)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].data == itemData)
                {
                    if (slots[i].amount < itemData.maxStackSize)
                    {
                        slots[i].amount += amount;
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // 空きスロットへ新規配置
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new ItemInstance(itemData, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false; // 空きなし
    }

    public bool RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex] == null)
            return false;

        slots[slotIndex].amount -= amount;
        if (slots[slotIndex].amount <= 0)
        {
            slots[slotIndex] = null;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public ItemInstance GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        return slots[slotIndex];
    }
}