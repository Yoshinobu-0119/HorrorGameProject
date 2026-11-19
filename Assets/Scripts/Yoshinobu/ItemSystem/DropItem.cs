using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class DropItem : MonoBehaviour
{
    [Header("アイテムデータ")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("ビジュアル参照")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Transform modelRoot; // 3Dモデルを生成する親Transform（未設定なら自身のTransform）

    [Header("設定")]
    [SerializeField] private float pickupDelay = 0.5f; // ドロップ直後に即再回収されるのを防ぐ時間
    private float spawnTime;
    private GameObject currentModelInstance;

    [Header("コライダー設定")]
    [SerializeField] private Collider defaultPhysicsCollider; // 元Prefabの物理用Collider（BoxColliderなど）
    [SerializeField] private SphereCollider pickupTriggerCollider;

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
        if (itemData == null) return;

        // 既存の3Dモデルがあれば削除
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
            currentModelInstance = null;
        }

        // 未割り当てなら自身の非Triggerコライダーを自動取得
        if (defaultPhysicsCollider == null)
        {
            foreach (var col in GetComponents<Collider>())
            {
                if (!col.isTrigger)
                {
                    defaultPhysicsCollider = col;
                    break;
                }
            }
        }

        // 3Dモデルが設定されている場合
        if (itemData.itemModel != null)
        {
            if (itemImage != null) itemImage.gameObject.SetActive(false);

            // 元Prefabの小さなBoxColliderをオフにする
            if (defaultPhysicsCollider != null) defaultPhysicsCollider.enabled = false;

            Transform parent = modelRoot != null ? modelRoot : transform;
            currentModelInstance = Instantiate(itemData.itemModel, parent);
            currentModelInstance.transform.localPosition = itemData.offsetPos;
            currentModelInstance.transform.localRotation = Quaternion.Euler(itemData.offsetRotation);
            currentModelInstance.transform.localScale = itemData.offsetScale;

            // 3Dモデル側のColliderを取得
            Collider[] modelColliders = currentModelInstance.GetComponentsInChildren<Collider>(true);

            // 1つもColliderが付いていない場合は、MeshFilterを持つオブジェクトすべてにMeshColliderを自動追加
            if (modelColliders.Length == 0)
            {
                MeshFilter[] meshFilters = currentModelInstance.GetComponentsInChildren<MeshFilter>(true);
                foreach (var mf in meshFilters)
                {
                    if (mf.sharedMesh != null)
                    {
                        MeshCollider addedCol = mf.gameObject.AddComponent<MeshCollider>();
                        addedCol.convex = true; // Rigidbodyと組み合わせるため必須
                    }
                }

                // 万が一MeshFilterすらない空モデルだった場合の保険
                if (meshFilters.Length == 0 && defaultPhysicsCollider != null)
                {
                    defaultPhysicsCollider.enabled = true;
                }
            }
            else
            {
                // すでにColliderが付いている場合はそれらを有効化
                foreach (var col in modelColliders)
                {
                    col.enabled = true;
                    if (col is MeshCollider meshCol)
                    {
                        meshCol.convex = true;
                    }
                }
            }
        }
        // 3Dモデルがない場合は従来のSprite＆元PrefabのBoxColliderを使用
        else
        {
            if (defaultPhysicsCollider != null) defaultPhysicsCollider.enabled = true;

            if (itemImage != null)
            {
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = itemData.icon;
            }
        }
    }

    // プレイヤーが触れた時の処理（自動回収）
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - spawnTime < pickupDelay) return;

        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            TryPickup(player, allowSwap: false, currentSlotIndex: -1);
        }
    }

    /// <summary>
    /// プレイヤーがインタラクトキーを押した時に外部から呼び出すメソッド
    /// </summary>
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
            spawnTime = Time.time;
            return;
        }

        // 2. 1個も入らなかった（インベントリがいっぱい）場合
        if (allowSwap && remainder == amount)
        {
            int swapAmount = Mathf.Min(amount, itemData.maxStackSize);
            int leftOnGround = amount - swapAmount;

            ItemInstance droppedFromPlayer = inventory.SwapItem(currentSlotIndex, itemData, swapAmount);

            if (droppedFromPlayer != null && droppedFromPlayer.data != null)
            {
                if (leftOnGround > 0)
                {
                    Instantiate(gameObject, transform.position, Quaternion.identity)
                        .GetComponent<DropItem>().Setup(droppedFromPlayer.data, droppedFromPlayer.amount);

                    amount = leftOnGround;
                    spawnTime = Time.time;
                }
                else
                {
                    Setup(droppedFromPlayer.data, droppedFromPlayer.amount);
                }
            }
        }
    }
}