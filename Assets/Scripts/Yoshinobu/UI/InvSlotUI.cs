using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    private int slotIndex;
    private MainGameUIManager uiManager;

    public void Initialize(int index, MainGameUIManager manager)
    {
        slotIndex = index;
        uiManager = manager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiManager?.OnSlotHoverEnter(slotIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiManager?.OnSlotHoverExit(slotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            uiManager?.OnSlotLeftClick(slotIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        uiManager?.OnSlotBeginDrag(slotIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // IDropHandler を機能させるために実装が必要
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        uiManager?.OnSlotEndDrag(slotIndex);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        uiManager?.OnSlotDrop(slotIndex);
    }
}