using System;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private int maxScrollSlots = 5;
    private ItemInstance[] slots;
    public int MaxSlots => maxSlots;
    public int MaxScrollSlots => maxScrollSlots;
    [SerializeField] public ItemInstance[] Slots => slots;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        slots = new ItemInstance[maxSlots];
    }

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        return AddItemAndGetRemainder(itemData, amount) == 0;
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

    /// <summary>
    /// アイテムを追加し、入り切らなかった「余りの個数」を返す（すべて入れば 0 を返す）
    /// </summary>
    public int AddItemAndGetRemainder(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0) return 0;
        int remaining = amount;
        bool changed = false;

        // 1. 既存スロットへのスタック処理
        if (itemData.maxStackSize > 1)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].data == itemData && slots[i].amount < itemData.maxStackSize)
                {
                    int space = itemData.maxStackSize - slots[i].amount;
                    int toAdd = Mathf.Min(space, remaining);

                    slots[i].amount += toAdd;
                    remaining -= toAdd;
                    changed = true;

                    if (remaining <= 0) break;
                }
            }
        }

        // 2. 空きスロットへの新規配置
        if (remaining > 0)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    int toAdd = Mathf.Min(itemData.maxStackSize, remaining);
                    slots[i] = new ItemInstance(itemData, toAdd);
                    remaining -= toAdd;
                    changed = true;

                    if (remaining <= 0) break;
                }
            }
        }

        if (changed) OnInventoryChanged?.Invoke();
        return remaining; // 入り切らなかった余りを返す
    }

    /// <summary>
    /// 指定スロットのアイテムを取り出し、新しいアイテムと入れ替える
    /// </summary>
    public ItemInstance SwapItem(int slotIndex, ItemData newItemData, int newAmount)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;

        ItemInstance oldItem = slots[slotIndex];
        slots[slotIndex] = new ItemInstance(newItemData, newAmount);
        OnInventoryChanged?.Invoke();
        return oldItem;
    }

    /// <summary>
    /// 指定スロットにアイテムを直接セットする（UI操作用）
    /// </summary>
    public void SetSlot(int index, ItemInstance item)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index] = item;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// スロットAからスロットBへアイテムを移動（同じなら合算、違うなら入れ替え）
    /// </summary>
    public void MoveOrSwapSlot(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= slots.Length || toIndex < 0 || toIndex >= slots.Length) return;

        ItemInstance fromItem = slots[fromIndex];
        ItemInstance toItem = slots[toIndex];

        if (fromItem == null) return;

        // 移動先が空ならそのまま移動
        if (toItem == null)
        {
            slots[toIndex] = fromItem;
            slots[fromIndex] = null;
        }
        // 同じアイテムかつスタック可能なら合算
        else if (fromItem.data == toItem.data && toItem.data.maxStackSize > 1 && toItem.amount < toItem.data.maxStackSize)
        {
            int space = toItem.data.maxStackSize - toItem.amount;
            int moveAmount = Mathf.Min(space, fromItem.amount);

            toItem.amount += moveAmount;
            fromItem.amount -= moveAmount;

            if (fromItem.amount <= 0)
            {
                slots[fromIndex] = null;
            }
        }
        // 違うアイテムなら入れ替え
        else
        {
            slots[toIndex] = fromItem;
            slots[fromIndex] = toItem;
        }

        OnInventoryChanged?.Invoke();
    }
}