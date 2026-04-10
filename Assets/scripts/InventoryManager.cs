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

        foreach (Item item in startingItems)
        {
            Additem(item);
        }

        UpdateUI();
    }

    private void Update()
    {
        // 1. Inventory megnyitása / bezárása Tab gombbal
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isInventoryOpen = !isInventoryOpen;
            UpdateUI();
        }

        // --- JAVÍTÁS: A számgombokat ide KIVETTÜK az 'if(isInventoryOpen)' blokkból! ---
        // Így mindig tudsz váltani a hotbaron, zárt inventorynál is.
        if (Input.inputString != "")
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number <= inventorySlots.Length)
            {
                ChangeSelectedSlot(number - 1);
            }
        }
        // ------------------------------------------------------------------------------

        // 2. Csak akkor tudunk eldobni tárgyat (Q), ha nyitva van és az egerünk rajta van
        if (isInventoryOpen)
        {
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
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }

        if (openInventoryButton != null)
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

    public void SpawnItemInWorld(Item item, Vector3? customPos = null)
    {
        Vector3 spawnPosition = customPos ?? (playerTransform.position + new Vector3(1, 0, 0));

        GameObject newItemGO = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
        PickableItem pickable = newItemGO.GetComponent<PickableItem>();

        if (pickable != null)
        {
            pickable.item = item;
        }

        // --- ÚJ RÉSZ: Az Order in Layer beállítása ---
        SpriteRenderer sr = newItemGO.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // 5-ös érték: biztosan a szalag (3) és a szalagon lévő érc (4) felett lesz
            sr.sortingOrder = 5;
        }
    }
    void ChangeSelectedSlot(int newSlot)
    {
        if (selectedSlot >= 0)
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
            if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackedItems && itemInSlot.item.stackable == true)
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
            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemGO = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGO.GetComponent<InventoryItem>();
        inventoryItem.InitializeItem(item);
    }

    public Item GetSelectedItem(bool use)
    {
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;
            if (use == true)
            {
                itemInSlot.count--;
                if (itemInSlot.count <= 0)
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

    // --- ÚJ CRAFTING FUNKCIÓK ---

    // 1. Megszámolja, hány darab van egy adott tárgyból a teljes inventoryban
    public int GetItemCount(Item itemToCheck)
    {
        int totalCount = 0;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.item == itemToCheck)
            {
                totalCount += itemInSlot.count;
            }
        }
        return totalCount;
    }

    // 2. Kivesz megadott mennyiségű tárgyat (a craftingkor használjuk)
    public void RemoveItem(Item itemToRemove, int amountToRemove)
    {
        int amountLeftToRemove = amountToRemove;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot != null && itemInSlot.item == itemToRemove)
            {
                if (itemInSlot.count >= amountLeftToRemove)
                {
                    // Ha ebben a slotban van elég (vagy több), levonjuk a maradékot és végzünk
                    itemInSlot.count -= amountLeftToRemove;
                    if (itemInSlot.count <= 0)
                    {
                        Destroy(itemInSlot.gameObject);
                    }
                    else
                    {
                        itemInSlot.RefreshCount();
                    }
                    return;
                }
                else
                {
                    // Ha nincs elég ebben a slotban, lenullázzuk, és a maradékot a következő slotból vonjuk le
                    amountLeftToRemove -= itemInSlot.count;
                    Destroy(itemInSlot.gameObject);
                }
            }
        }
    }
}