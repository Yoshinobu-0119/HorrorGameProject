using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using LitMotion;
using LitMotion.Extensions;

public class MainGameUIManager : MonoBehaviour
{
    [Header("プレイヤー参照")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventorySystem inventory;

    [Space(15)]
    [Header("スロットUI参照 (0~4: ホットバー / 5~25: インベントリ)")]
    [SerializeField] private Transform[] itemSlotsPos;
    [SerializeField] private Transform selectedImg;

    [Header("インベントリ開閉設定")]
    [SerializeField] private RectTransform inventoryPanel; // Slot5~25の親Panel
    [SerializeField] private float hiddenYOffset = -800f;  // 隠すときのY座標オフセット
    [SerializeField] private float panelAnimDuration = 0.25f;
    [SerializeField] private Ease panelEase = Ease.OutExpo;

    [Header("掴み中アイテム表示 (カーソル追従用)")]
    [SerializeField] private RectTransform heldItemRoot;   // カーソルに追従するUIオブジェクト
    [SerializeField] private Image heldItemIcon;
    [SerializeField] private TextMeshProUGUI heldItemAmountText;

    [Header("選択枠アニメーション設定")]
    [SerializeField] private float selectMoveDuration = 0.15f;
    [SerializeField] private Ease selectEase = Ease.OutExpo;

    // キャッシュ
    private Image[] slotIcons;
    private TextMeshProUGUI[] slotAmountTexts;

    // 状態管理
    private bool isInventoryOpen = false;
    private Vector2 shownPanelPos;
    private Vector2 hiddenPanelPos;
    private int lastSelectedIndex = -1;
    private int hoveredSlotIndex = -1;
    private int lastHoveredInInventory = 0;

    // クリック掴み / ドラッグ中の状態
    private ItemInstance heldItem = null;
    private int draggingFromIndex = -1;

    private MotionHandle selectPosHandle;
    private MotionHandle selectScaleHandle;
    private MotionHandle panelMoveHandle;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<Player>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (inventory == null) inventory = FindFirstObjectByType<InventorySystem>();

        CacheAndSetupSlots();

        if (inventoryPanel != null)
        {
            shownPanelPos = inventoryPanel.anchoredPosition;
            hiddenPanelPos = shownPanelPos + new Vector2(0f, hiddenYOffset);
            // 初期状態は画面下に隠しておく
            inventoryPanel.anchoredPosition = hiddenPanelPos;
        }

        UpdateHeldItemVisual();
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.OnInventoryChanged += UpdateAllSlotsUI;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnInventoryChanged -= UpdateAllSlotsUI;
    }

    private void Start()
    {
        UpdateAllSlotsUI();

        if (playerController != null && itemSlotsPos.Length > 0)
        {
            int initIndex = Mathf.Clamp(playerController.SelectedSlotIndex, 0, itemSlotsPos.Length - 1);
            lastSelectedIndex = initIndex;
            if (selectedImg != null)
            {
                selectedImg.position = itemSlotsPos[initIndex].position;
                selectedImg.localScale = itemSlotsPos[initIndex].localScale;
            }
        }
    }

    private void Update()
    {
        // 1. 選択枠の追従ターゲット決定
        if (isInventoryOpen)
        {
            if (hoveredSlotIndex >= 0)
            {
                lastHoveredInInventory = hoveredSlotIndex;
            }
            // インベントリ開放中は、スロット間の隙間を通っても下のホットバーに勝手に戻らない
            int targetIndex = lastHoveredInInventory;
            if (targetIndex != lastSelectedIndex && targetIndex >= 0 && targetIndex < itemSlotsPos.Length)
            {
                MoveSelectedIndicator(targetIndex);
                lastSelectedIndex = targetIndex;
            }
        }
        else
        {
            // インベントリを閉じている時は通常通りホットバーの選択スロットへ
            int targetIndex = playerController != null ? playerController.SelectedSlotIndex : 0;
            if (targetIndex != lastSelectedIndex && targetIndex >= 0 && targetIndex < itemSlotsPos.Length)
            {
                MoveSelectedIndicator(targetIndex);
                lastSelectedIndex = targetIndex;
            }
        }

        // 2. 掴んでいるアイテムがあればマウスカーソル位置に追従
        if (heldItem != null && heldItemRoot != null && Mouse.current != null)
        {
            heldItemRoot.position = Mouse.current.position.ReadValue();
        }

        // 3. インベントリ画面を開いている時の Q キー（ドロップ）処理
        if (isInventoryOpen && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            HandleInventoryDropInput();
        }
    }

    /// <summary>
    /// InputSystem の "Inventory" (Tabキー) から呼び出されるコールバック
    /// ※PlayerInput が UI 側ではなく Player 側にある場合は Player.cs からこのメソッドを呼んでください
    /// </summary>
    public void OnInventory(InputValue value)
    {
        if (!value.isPressed) return;
        ToggleInventory();
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (playerController != null)
        {
            playerController.SetInventoryOpenState(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            // 開いた瞬間は現在選択中のホットバースロット位置からスタート
            lastHoveredInInventory = playerController != null ? playerController.SelectedSlotIndex : 0;
        }
        else
        {
            // 閉じるときにアイテムを掴んだままならインベントリに戻す
            ReturnHeldItemToInventory();
            hoveredSlotIndex = -1;
            draggingFromIndex = -1;
        }

        // LitMotion で画面下へスライドイン / スライドアウト
        if (inventoryPanel != null)
        {
            if (panelMoveHandle.IsActive()) panelMoveHandle.Cancel();

            Vector2 targetPos = isInventoryOpen ? shownPanelPos : hiddenPanelPos;
            panelMoveHandle = LMotion.Create(inventoryPanel.anchoredPosition, targetPos, panelAnimDuration)
                .WithEase(panelEase)
                .BindToAnchoredPosition(inventoryPanel);
        }
    }

    private void CacheAndSetupSlots()
    {
        int count = itemSlotsPos.Length;
        slotIcons = new Image[count];
        slotAmountTexts = new TextMeshProUGUI[count];

        for (int i = 0; i < count; i++)
        {
            Transform slotTr = itemSlotsPos[i];
            if (slotTr == null) continue;

            // マウスイベント検知コンポーネントを自動付与して初期化
            if (!slotTr.TryGetComponent<InventorySlotUI>(out var slotUI))
            {
                slotUI = slotTr.gameObject.AddComponent<InventorySlotUI>();
            }
            slotUI.Initialize(i, this);

            // アイコン用Image取得
            Transform iconTr = slotTr.Find("Icon");
            if (iconTr != null)
            {
                slotIcons[i] = iconTr.GetComponent<Image>();
            }
            else
            {
                Image[] imgs = slotTr.GetComponentsInChildren<Image>(true);
                if (imgs.Length > 1) slotIcons[i] = imgs[1];
            }

            // 個数テキスト取得
            slotAmountTexts[i] = slotTr.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    public void UpdateAllSlotsUI()
    {
        if (inventory == null) return;

        for (int i = 0; i < itemSlotsPos.Length; i++)
        {
            ItemInstance item = inventory.GetItem(i);
            bool hasItem = (item != null && item.data != null && item.amount > 0);

            if (slotIcons[i] != null)
            {
                slotIcons[i].sprite = hasItem ? item.data.icon : null;
                slotIcons[i].enabled = hasItem;
            }

            if (slotAmountTexts[i] != null)
            {
                if (hasItem && item.amount > 1)
                {
                    slotAmountTexts[i].text = item.amount.ToString();
                    slotAmountTexts[i].enabled = true;
                }
                else
                {
                    slotAmountTexts[i].text = "";
                    slotAmountTexts[i].enabled = false;
                }
            }
        }
    }

    private void MoveSelectedIndicator(int targetIndex)
    {
        if (selectedImg == null || targetIndex < 0 || targetIndex >= itemSlotsPos.Length) return;

        if (selectPosHandle.IsActive()) selectPosHandle.Cancel();
        if (selectScaleHandle.IsActive()) selectScaleHandle.Cancel();

        Transform targetSlot = itemSlotsPos[targetIndex];

        selectPosHandle = LMotion.Create(selectedImg.position, targetSlot.position, selectMoveDuration)
            .WithEase(selectEase)
            .BindToPosition(selectedImg);

        // Slot5~25 は Scale が小さいため、選択枠のサイズも合わせる
        selectScaleHandle = LMotion.Create(selectedImg.localScale, targetSlot.localScale, selectMoveDuration)
            .WithEase(selectEase)
            .BindToLocalScale(selectedImg);
    }

    // =========================================================
    // スロット操作ロジック（ホバー・クリック・ドラッグ＆ドロップ）
    // =========================================================

    public void OnSlotHoverEnter(int index)
    {
        if (!isInventoryOpen) return;
        hoveredSlotIndex = index;
    }

    public void OnSlotHoverExit(int index)
    {
        if (hoveredSlotIndex == index)
        {
            hoveredSlotIndex = -1;
        }
    }

    /// <summary>
    /// 左クリック：掴む / 置く / 合算 / 入れ替え / Shift+左クリックで半分掴む
    /// </summary>
    public void OnSlotLeftClick(int index)
    {
        if (!isInventoryOpen || inventory == null) return;
        if (draggingFromIndex != -1) return; // ドラッグ直後の誤発火防止

        bool isShift = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        ItemInstance slotItem = inventory.GetItem(index);

        // 1. 何も掴んでいない状態 -> スロットからアイテムを掴む
        if (heldItem == null)
        {
            if (slotItem == null || slotItem.data == null) return;

            if (isShift && slotItem.amount > 1)
            {
                // Shift + 左クリック：半分（切り上げ）だけ掴む
                int takeAmount = Mathf.CeilToInt(slotItem.amount / 2f);
                int remainAmount = slotItem.amount - takeAmount;

                heldItem = new ItemInstance(slotItem.data, takeAmount);
                inventory.RemoveItem(index, takeAmount);
            }
            else
            {
                // 通常左クリック：すべて掴む
                heldItem = new ItemInstance(slotItem.data, slotItem.amount);
                inventory.SetSlot(index, null);
            }
        }
        // 2. すでにアイテムを掴んでいる状態 -> スロットへ置く / 合算 / 入れ替え
        else
        {
            if (slotItem == null || slotItem.data == null)
            {
                // 空スロットならそのまま置く
                inventory.SetSlot(index, heldItem);
                heldItem = null;
            }
            else if (slotItem.data == heldItem.data && slotItem.data.maxStackSize > 1 && slotItem.amount < slotItem.data.maxStackSize)
            {
                // 同じアイテムなら maxStackSize まで合算し、余りは掴んだまま残す
                int space = slotItem.data.maxStackSize - slotItem.amount;
                int addAmount = Mathf.Min(space, heldItem.amount);

                slotItem.amount += addAmount;
                heldItem.amount -= addAmount;

                if (heldItem.amount <= 0) heldItem = null;
                inventory.SetSlot(index, slotItem); // 更新通知を発火
            }
            else
            {
                // 違うアイテム（または満タン）なら入れ替え
                ItemInstance temp = new ItemInstance(slotItem.data, slotItem.amount);
                inventory.SetSlot(index, heldItem);
                heldItem = temp;
            }
        }

        UpdateHeldItemVisual();
    }

    // --- ドラッグ＆ドロップ ---

    public void OnSlotBeginDrag(int index)
    {
        if (!isInventoryOpen || inventory == null) return;
        if (heldItem != null) return; // すでにクリックで掴んでいる場合はそちらを優先

        ItemInstance slotItem = inventory.GetItem(index);
        if (slotItem == null || slotItem.data == null) return;

        draggingFromIndex = index;
        heldItem = new ItemInstance(slotItem.data, slotItem.amount);
        inventory.SetSlot(index, null);
        UpdateHeldItemVisual();
    }

    public void OnSlotDrop(int targetIndex)
    {
        if (!isInventoryOpen || inventory == null) return;
        if (draggingFromIndex == -1 || heldItem == null) return;

        // 元のスロットに一旦戻してから MoveOrSwapSlot で合算・入れ替えを処理
        inventory.SetSlot(draggingFromIndex, heldItem);
        heldItem = null;
        UpdateHeldItemVisual();

        inventory.MoveOrSwapSlot(draggingFromIndex, targetIndex);
        draggingFromIndex = -1;
    }

    public void OnSlotEndDrag(int index)
    {
        // スロット以外の場所（画面外など）でドロップされた場合は元のスロットに戻す
        if (draggingFromIndex != -1 && heldItem != null)
        {
            inventory.SetSlot(draggingFromIndex, heldItem);
            heldItem = null;
            UpdateHeldItemVisual();
        }
        draggingFromIndex = -1;
    }

    /// <summary>
    /// インベントリを開いている間の Q キーによるドロップ処理
    /// </summary>
    private void HandleInventoryDropInput()
    {
        if (player == null || inventory == null) return;
        bool isCtrl = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed; // Ctrl+Q で全捨て（単押しQで1個捨て）

        // 1. アイテムをカーソルで掴んでいる場合 -> 掴んでいるアイテムから捨てる
        if (heldItem != null)
        {
            int dropCount = isCtrl ? heldItem.amount : 1;
            player.DropItemDirect(heldItem.data, dropCount);
            heldItem.amount -= dropCount;

            if (heldItem.amount <= 0) heldItem = null;
            UpdateHeldItemVisual();
            return;
        }

        // 2. スロットにカーソルを合わせている場合 -> そのスロットから捨てる
        if (hoveredSlotIndex >= 0 && hoveredSlotIndex < itemSlotsPos.Length)
        {
            ItemInstance targetItem = inventory.GetItem(hoveredSlotIndex);
            if (targetItem != null && targetItem.data != null && targetItem.amount > 0)
            {
                int dropCount = isCtrl ? targetItem.amount : 1;
                ItemData data = targetItem.data;

                inventory.RemoveItem(hoveredSlotIndex, dropCount);
                player.DropItemDirect(data, dropCount);
            }
        }
    }

    private void ReturnHeldItemToInventory()
    {
        if (heldItem == null || inventory == null) return;

        int remainder = inventory.AddItemAndGetRemainder(heldItem.data, heldItem.amount);
        if (remainder > 0 && player != null)
        {
            // インベントリが満杯で戻せない場合は足元にドロップ
            player.DropItemDirect(heldItem.data, remainder);
        }

        heldItem = null;
        UpdateHeldItemVisual();
    }

    private void UpdateHeldItemVisual()
    {
        if (heldItemRoot == null) return;

        bool hasHeld = (heldItem != null && heldItem.data != null && heldItem.amount > 0);
        heldItemRoot.gameObject.SetActive(hasHeld);

        if (hasHeld)
        {
            if (heldItemIcon != null) heldItemIcon.sprite = heldItem.data.icon;
            if (heldItemAmountText != null)
            {
                heldItemAmountText.text = heldItem.amount > 1 ? heldItem.amount.ToString() : "";
            }
        }
    }

    private void OnDestroy()
    {
        if (selectPosHandle.IsActive()) selectPosHandle.Cancel();
        if (selectScaleHandle.IsActive()) selectScaleHandle.Cancel();
        if (panelMoveHandle.IsActive()) panelMoveHandle.Cancel();
    }
}