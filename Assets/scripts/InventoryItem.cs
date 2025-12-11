using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// FONTOS: Add hozzá az IPointerEnterHandler és IPointerExitHandler interfészeket a felsoroláshoz!
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public Image image;
    public Text countText;  

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;

    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = item.image;
        RefreshCount();
    }

    public void RefreshCount()
    {
       countText.text = count.ToString();
       bool textActive = count > 1;
       countText.gameObject.SetActive(textActive);
    }

    // --- ÚJ RÉSZ: EGÉR ÉRZÉKELÉSE ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ha az egér ráment a tárgyra, szólunk a Managernek
        InventoryManager.instance.hoveredItem = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Ha az egér lement róla, és még mindig ez volt a kiválasztott, töröljük
        if (InventoryManager.instance.hoveredItem == this)
        {
            InventoryManager.instance.hoveredItem = null;
        }
    }
    // --------------------------------

    // Drag and drop metódusok (maradnak a régiek vagy a módosítottak)
    public void OnBeginDrag(PointerEventData eventData)
    {
       image.raycastTarget = false;
       parentAfterDrag = transform.parent;
       transform.SetParent(transform.root);
       transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
       transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }
}