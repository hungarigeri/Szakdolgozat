using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour,IDropHandler
{
    public Image Image;
    public Color SelectedColor,notSelectedColor;

    public void Start()
    {
        Deselect();
    }

    public void Select()
    {
        Image.color = SelectedColor;
    }
    public void Deselect()
    {
        Image.color = notSelectedColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            InventoryItem inventoryItem = dropped.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
      
    }
}
