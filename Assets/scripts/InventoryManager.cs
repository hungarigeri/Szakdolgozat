using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI Reference")]
    public GameObject inventoryUI;
    public GameObject openInventoryButton;

    // Hiába írod ide, hogy false, az Inspector felülírhatja, ezért majd a Start-ban fixáljuk
    public bool isInventoryOpen = false; 

    public int maxStackedItems = 64;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

    [Header("Starting Items")]
    public Item[] startingItems;

    [Header("Dropping")]
    public GameObject droppedItemPrefab;
    public Transform playerTransform;

    [HideInInspector] public InventoryItem hoveredItem; 
    
    int selectedSlot = -1;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // --- JAVÍTÁS 1: Kényszerítjük, hogy ZÁRVA induljon ---
        isInventoryOpen = false; 
        // ----------------------------------------------------

        if (playerTransform == null)
        {
             Player playerScript = FindFirstObjectByType<Player>();
             if (playerScript != null) playerTransform = playerScript.transform;
        }

        ChangeSelectedSlot(0);

        foreach(Item item in startingItems)
        {
            Additem(item);
        }
        
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isInventoryOpen = !isInventoryOpen;
            UpdateUI();
        }

        if (isInventoryOpen)
        {
            if(Input.inputString != "")
            {
                bool isNumber = int.TryParse(Input.inputString, out int number);
                if(isNumber && number > 0 && number <= inventorySlots.Length)
                {
                    ChangeSelectedSlot(number - 1);
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (hoveredItem != null)
                {
                    DropHoveredItem();
                }
            }
        }
    }

    void UpdateUI()
    {
        if(inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }
        
        if(openInventoryButton != null)
        {
            openInventoryButton.SetActive(!isInventoryOpen);
        }
    }

    // --- JAVÍTÁS 2: Itt true-ra kell állítani, hogy KINYÍLJON ---
    public void OpenInventoryByButton()
    {
        isInventoryOpen = true; // Eredetileg false volt, ami rossz!
        UpdateUI();
    }
    // -----------------------------------------------------------

    void DropHoveredItem()
    {
        SpawnItemInWorld(hoveredItem.item);
        hoveredItem.count--;
        
        if (hoveredItem.count <= 0)
        {
            Destroy(hoveredItem.gameObject);
            hoveredItem = null;
        }
        else
        {
            hoveredItem.RefreshCount();
        }
    }

    public void SpawnItemInWorld(Item item)
    {
       Vector3 spawnPosition = playerTransform.position + new Vector3(1, 0, 0); 
       GameObject newItemGO = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
       PickableItem pickable = newItemGO.GetComponent<PickableItem>();
       
       if(pickable != null) 
       {
           pickable.item = item;
       }
    }

    void ChangeSelectedSlot(int newSlot)
    {  
        if(selectedSlot >= 0)
        {
              inventorySlots[selectedSlot].Deselect();
        }
       inventorySlots[newSlot].Select();
       selectedSlot = newSlot;
    }

    public bool Additem(Item item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackedItems && itemInSlot.item.stackable == true)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null)
            {
                SpawnNewItem(item,slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item item,InventorySlot slot)
    {
        GameObject newItemGO = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGO.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item); 
    }

    public Item GetSelectedItem(bool use)
    {
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if(itemInSlot != null)
        {
            Item item = itemInSlot.item;
            if(use == true)
            {
                itemInSlot.count--;
                if(itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    itemInSlot.RefreshCount();
                }
            }
            return item;
        }
        return null;
    }
}