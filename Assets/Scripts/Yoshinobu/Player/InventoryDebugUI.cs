using UnityEngine;

public class InventoryDebugUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<Player>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnGUI()
    {
        if (player == null || player.Inventory == null || playerController == null) return;

        InventorySystem inventory = player.Inventory;
        int selectedIndex = playerController.SelectedSlotIndex;

        // UI表示用の領域とスタイル設定
        GUILayout.BeginArea(new Rect(20, 20, 300, 500), GUI.skin.box);
        GUILayout.Label("<b>--- INVENTORY ---</b>");

        for (int i = 0; i < inventory.MaxSlots; i++)
        {
            ItemInstance item = inventory.GetItem(i);
            bool isSelected = (i == selectedIndex);

            // 選択中のスロットを強調
            string prefix = isSelected ? "<color=yellow>[->] </color>" : "     ";
            string itemText = (item != null && item.data != null)
                ? $"{item.data.itemName} x{item.amount}"
                : "<Empty>";

            GUIStyle style = new GUIStyle(GUI.skin.label) { richText = true };
            GUILayout.Label($"{prefix}Slot {i:D2}: {itemText}", style);
        }

        GUILayout.EndArea();
    }
}