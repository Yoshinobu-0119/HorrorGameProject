using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("コンポーネント参照")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private PlayerController playerController; // 選択スロット取得用
    [SerializeField] private Transform attackOrigin; // カメラや銃口の位置

    [Header("ステータス")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    [Header("インタラクト設定")]
    [SerializeField] private float interactRange = 3.0f; // アイテムを拾える距離
    [SerializeField] private LayerMask interactLayer = ~0;

    [Header("アイテムドロップ設定")]
    [SerializeField] private DropItem dropItemPrefab; // 生成するDropItemのPrefab
    [SerializeField] private float dropForwardOffset = 1.0f; // プレイヤーのどれくらい前に生成するか
    [SerializeField] private float dropThrowForce = 4.0f; // 前方に軽く放り投げる力
    [SerializeField] private float holdDurationToDropAll = 0.5f; // 全ドロップになる長押し時間（秒）

    private bool _isHoldingDrop;
    private float _dropHoldTimer;
    private bool _droppedAllThisHold;

    public InventorySystem Inventory => inventory;

    private void Awake()
    {
        if (inventory == null) inventory = GetComponent<InventorySystem>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // 長押し判定：規定時間に達した瞬間に全ドロップを実行（指を離す前でも反応する）
        if (_isHoldingDrop && !_droppedAllThisHold)
        {
            _dropHoldTimer += Time.deltaTime;
            if (_dropHoldTimer >= holdDurationToDropAll)
            {
                _droppedAllThisHold = true;
                DropSelectedSlotItem(dropAll: true);
            }
        }
    }

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
                if (item.data.reduceAmountOnUse)
                    inventory.RemoveItem(slotIndex, 1);
            }
        }
    }

    /// <summary>
    /// InputSystem の "Interact" アクションから自動呼び出しされるコールバック
    /// </summary>
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        // PlayerController側の選択スロットを取得（なければ0）
        int slotIndex = playerController != null ? playerController.SelectedSlotIndex : 0;

        // 視点（attackOrigin）から前方にRaycastを飛ばす
        Ray ray = new Ray(attackOrigin.position, attackOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            if (hit.collider.TryGetComponent<DropItem>(out var dropItem))
            {
                // 視線の先にDropItemがあれば回収（または手持ちと入れ替え）を実行
                dropItem.OnInteract(this, slotIndex);
            }
        }
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red,3f);
    }

    /// <summary>
    /// InputSystem の "ItemDrop" アクションから呼び出されるコールバック
    /// ※InputActions側で Press Interaction (Press And Release) を設定してください
    /// </summary>
    public void OnItemDrop(InputValue value)
    {
        if (value.isPressed)
        {
            // 押し始めた瞬間
            _isHoldingDrop = true;
            _dropHoldTimer = 0f;
            _droppedAllThisHold = false;
        }
        else
        {
            // ボタンを離した瞬間
            if (_isHoldingDrop && !_droppedAllThisHold)
            {
                // 長押し時間に達する前に離したので「単押し（1個ドロップ）」
                DropSelectedSlotItem(dropAll: false);
            }
            _isHoldingDrop = false;
            _dropHoldTimer = 0f;
        }
    }

    /// <summary>
    /// 現在選択中のスロットのアイテムをドロップする
    /// </summary>
    private void DropSelectedSlotItem(bool dropAll)
    {
        int slotIndex = playerController != null ? playerController.SelectedSlotIndex : 0;
        ItemInstance item = inventory.GetItem(slotIndex);

        if (item == null || item.data == null) return;

        // ★1. インベントリから削除する前に、データと個数を変数に控えておく
        ItemData targetData = item.data;
        int currentAmount = item.amount; // ※個数の変数名が count や stack の場合は変更してください
        if (currentAmount <= 0) return;

        int dropCount = dropAll ? currentAmount : 1;

        // 2. インベントリから減らす
        inventory.RemoveItem(slotIndex, dropCount);

        // 3. 控えておいたデータと個数を渡して生成する
        SpawnDropItem(targetData, dropCount);
    }

    private void SpawnDropItem(ItemData itemData, int amount)
    {
        if (dropItemPrefab == null) return;

        Vector3 spawnPos = attackOrigin.position + attackOrigin.forward * dropForwardOffset;
        DropItem dropped = Instantiate(dropItemPrefab, spawnPos, Quaternion.identity);

        // ★ここでプレハブの初期値を今捨てたアイテム・個数で上書きする！
        dropped.Initialize(itemData, amount);

        if (dropped.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 throwDir = (attackOrigin.forward + Vector3.up * 0.25f).normalized;
            rb.linearVelocity = throwDir * dropThrowForce;
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