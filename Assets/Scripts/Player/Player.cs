using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("コンポーネント参照")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Transform attackOrigin; // カメラや銃口の位置

    [Header("ステータス")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    public InventorySystem Inventory => inventory;

    // 特定スロットのアイテムを使用
    public void UseItem(int slotIndex)
    {
        ItemInstance item = inventory.GetItem(slotIndex);
        if (item == null || item.data == null) return;

        foreach (var action in item.data.actions)
        {
            bool success = action.Execute(this, item);
            if (success && item.data.maxStackSize > 1)
            {
                inventory.RemoveItem(slotIndex, 1);
            }
        }
    }

    // 各アクションから呼び出される具体的な受け皿ロジック
    public void Heal(float amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        Debug.Log($"HP回復: {amount} / 現在HP: {currentHp}");
    }

    public void PerformRangedAttack(int damage, float range, GameObject bulletPrefab)
    {
        Debug.Log($"[銃] 威力: {damage} / 射程: {range}");
        Ray ray = new Ray(attackOrigin.position, attackOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log($"着弾: {hit.collider.name}");
        }
    }

    public void PerformMeleeAttack(int damage, float radius, string animTrigger)
    {
        Debug.Log($"[ナイフ] 威力: {damage} / 範囲: {radius}");
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position + attackOrigin.forward, radius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                Debug.Log($"近接ヒット: {hit.name}");
            }
        }
    }
}