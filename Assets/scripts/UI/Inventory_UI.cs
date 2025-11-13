using System.Collections.Generic;
using UnityEngine;

public class Inventory_UI : MonoBehaviour
{
    public GameObject inventoryPanel;

    public Player player;

    public List<Slot_UI> slotUIs = new List<Slot_UI>();

  

    
    void Start()
    {
        // ensure the inventory is closed at start
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        // optional: initialize UI state
        Setup();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            toggleInventory();
        }
    }

    public void toggleInventory()
    {
        if (inventoryPanel == null) return;

        bool willOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(willOpen);

        if (willOpen)
            Setup();
    }


    void Setup()
    {
       if(slotUIs.Count == player.inventory.slots.Count)
       {
            for (int i = 0; i < slotUIs.Count; i++)
            {
                
                if (player.inventory.slots[i].type != CollectableType.None)
                {
                   slotUIs[i].SetItem(player.inventory.slots[i]);
                
                }
                else
                {
                    slotUIs[i].SetEmpty();
                }
            }
       }
    }
}
