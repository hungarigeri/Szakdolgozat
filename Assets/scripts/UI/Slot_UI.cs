using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot_UI : MonoBehaviour
{
    public Image ItemIcon;
    public TextMeshProUGUI QuantityText;

    public void SetItem(Inventory.Slot slot)
    {
        if (slot != null)
        {
            ItemIcon.sprite = slot.icon;
            ItemIcon.color = new Color(1, 1, 1, 1);
            QuantityText.text = slot.count.ToString();
        }
    }
    
    public void SetEmpty()
    {
        ItemIcon.sprite = null;
        ItemIcon.color = new Color(1, 1, 1, 0);
        QuantityText.text = "";
    }

    
}
