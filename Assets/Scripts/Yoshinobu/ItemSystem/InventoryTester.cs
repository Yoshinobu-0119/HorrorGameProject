// テスト用に起動時アイテムを追加するコンポーネント
using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private ItemData gunData;
    [SerializeField] private ItemData knifeData;
    [SerializeField] private ItemData potionData;

    void Start()
    {
        if (inventory == null) inventory = GetComponent<InventorySystem>();

        // スロット0に銃、スロット1にナイフ、スロット2にポーションをセット
        inventory.AddItem(gunData, 1);
        inventory.AddItem(knifeData, 1);
        inventory.AddItem(potionData, 3);
    }
}