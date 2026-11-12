// テスト用に起動時アイテムを追加するコンポーネント
using UnityEngine;

[System.Serializable]
public struct addItemData
{
    public ItemData item;
    public int amount;
}


public class InventoryTester : MonoBehaviour
{
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private addItemData[] addItemData;
    [SerializeField] private addItemData[] addItemDataKey;  //キー入力でデバッグする時のアイテム追加処理

    void Start()
    {
        if (inventory == null) inventory = GetComponent<InventorySystem>();

        
        for (int i = 0; i < addItemData.Length; i++)
        {
            inventory.AddItem(addItemData[i].item, addItemData[i].amount);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            for (int i = 0; i < addItemDataKey.Length; i++)
            {
                inventory.AddItem(addItemDataKey[i].item, addItemDataKey[i].amount);
            }
        }
    }
}