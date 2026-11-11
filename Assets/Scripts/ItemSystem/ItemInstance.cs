using UnityEngine;

/// <summary>
/// 耐久値や現在所持数などの動的データを保持するクラス
/// </summary>
[System.Serializable]
public class ItemInstance
{
    public ItemData data;
    public int amount;
    public int currentDurability;

    public ItemInstance(ItemData data, int amount = 1)
    {
        this.data = data;
        this.amount = amount;
    }

    public int GetSellValue()
    {
        return data != null ? data.baseSellValue : 0;
    }
}