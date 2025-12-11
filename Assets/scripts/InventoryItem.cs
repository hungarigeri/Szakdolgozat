using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    //drag and drop interface methods
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
