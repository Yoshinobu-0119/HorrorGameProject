using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class DropItem : MonoBehaviour
{
    [Header("アイテムデータ")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("UI参照")]
    [SerializeField] private Image itemImage;

    [Header("設定")]
    [SerializeField] private float pickupDelay = 0.5f; // ドロップ直後に即再回収されるのを防ぐ時間
    private float spawnTime;

    private void Awake()
    {
        spawnTime = Time.time;
        UpdateVisual();
    }

    public void Initialize(ItemData data, int dropAmount)
    {
        this.itemData = data;
        this.amount = dropAmount;
        Setup(data, dropAmount);
    }

    // インスペクター上で値を変えた時にも即座に画像をプレビュー反映する
    private void OnValidate()
    {
        UpdateVisual();
    }

    /// <summary>
    /// 外部（ドロップ生成時など）からデータをセットする用
    /// </summary>
    public void Setup(ItemData data, int itemAmount)
    {
        itemData = data;
        amount = itemAmount;
        spawnTime = Time.time;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (itemImage != null && itemData != null)
        {
            itemImage.sprite = itemData.icon;
        }
    }

    // プレイヤーが触れた時の処理（自動回収）
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - spawnTime < pickupDelay) return;

        // 親オブジェクトに Player が付いていても取得できるようにする
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            Debug.Log("Player接触");
            TryPickup(player, allowSwap: false, currentSlotIndex: -1);
        }
    }

    /// <summary>
    /// プレイヤーがインタラクトキーを押した時に外部から呼び出すメソッド
    /// </summary>
    /// <param name="player">インタラクトしたPlayer</param>
    /// <param name="equippedSlotIndex">現在Playerが選択（装備）しているスロット番号</param>
    public void OnInteract(Player player, int equippedSlotIndex = 0)
    {
        if (itemData == null || amount <= 0) return;

        TryPickup(player, allowSwap: true, currentSlotIndex: equippedSlotIndex);
    }

    private void TryPickup(Player player, bool allowSwap, int currentSlotIndex)
    {
        if (itemData == null || amount <= 0) return;

        InventorySystem inventory = player.Inventory;
        if (inventory == null) return;

        // 1. まずインベントリに入るだけ入れる
        int remainder = inventory.AddItemAndGetRemainder(itemData, amount);

        // すべて入った場合
        if (remainder == 0)
        {
            Destroy(gameObject);
            return;
        }

        // 一部だけ入った場合（余りとして個数を更新してその場に残す）
        if (remainder < amount)
        {
            amount = remainder;
            spawnTime = Time.time; // 連続ヒット防止
            return;
        }

        // 2. 1個も入らなかった（インベントリがいっぱい）場合
        if (allowSwap && remainder == amount)
        {
            // maxStackSizeを超える塊を拾おうとした場合は、maxStackSize分だけ手持ちと交換し、残りは床に残す
            int swapAmount = Mathf.Min(amount, itemData.maxStackSize);
            int leftOnGround = amount - swapAmount;

            ItemInstance droppedFromPlayer = inventory.SwapItem(currentSlotIndex, itemData, swapAmount);

            if (droppedFromPlayer != null && droppedFromPlayer.data != null)
            {
                if (leftOnGround > 0)
                {
                    // 床に余りが残る場合は、プレイヤーが持っていたアイテムを新規Prefabとして足元に生成
                    Instantiate(gameObject, transform.position, Quaternion.identity)
                        .GetComponent<DropItem>().Setup(droppedFromPlayer.data, droppedFromPlayer.amount);

                    amount = leftOnGround;
                    spawnTime = Time.time;
                }
                else
                {
                    // 余りがなければ、このDropItem自体の中身をプレイヤーが手放したアイテムに書き換える（生成コスト削減）
                    Setup(droppedFromPlayer.data, droppedFromPlayer.amount);
                }
            }
        }
    }
}