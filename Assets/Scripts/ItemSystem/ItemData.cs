using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムのマスターデータ（IDやアイコンなどを格納）する
/// クラス
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite icon;
    public int baseSellValue;
    public int maxStackSize = 1;

    // ポリモーフィズム対応（インスペクターでアクションを追加・変更可能）
    [SerializeReference] public List<IItemAction> actions = new();
}