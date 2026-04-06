using UnityEngine;

public class FarmingSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject highlightSquare;

    [Header("Item References")]
    public Item ironOreItem;
    public Item woodItem;

    private Vector2 lastFacingDirection = Vector2.down;
    private SpriteRenderer highlightSR;

    void Start()
    {
        // Megpróbáljuk lekérni a SpriteRenderert a színezéshez
        if (highlightSquare != null)
        {
            highlightSR = highlightSquare.GetComponent<SpriteRenderer>();

            // Ha nincs rajta SpriteRenderer, szólunk a Console-ban, de nem omlasztjuk össze a játékot!
            if (highlightSR == null)
            {
                Debug.LogWarning("Figyelem: A Kijelölő Négyzeten nincs SpriteRenderer! A zöld/piros színváltás nem fog működni.");
            }
        }
    }

    void Update()
    {
        // --- BIZTONSÁGI ELLENŐRZÉSEK ---
        // Ha ezek közül bármelyik hiányzik, megállítjuk az Update-et, így elkerüljük az összeomlást
        if (InventoryManager.instance == null || GameManager.Instance == null || GameManager.Instance.tileManager == null)
            return;

        // 1. Irányítás
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (moveX != 0 || moveY != 0)
        {
            if (Mathf.Abs(moveX) > Mathf.Abs(moveY)) lastFacingDirection = new Vector2(Mathf.Sign(moveX), 0);
            else lastFacingDirection = new Vector2(0, Mathf.Sign(moveY));
        }

        Vector3Int targetPosition = new Vector3Int(
            Mathf.FloorToInt(transform.position.x + lastFacingDirection.x),
            Mathf.FloorToInt(transform.position.y + lastFacingDirection.y),
            0
        );

        // 2. Mi van a kezünkben?
        Item selectedItem = InventoryManager.instance.GetSelectedItem(false);
        bool isBuildingHeld = false;
        if (selectedItem != null && selectedItem.name == "CraftingTable") // FONTOS: Ez pontosan egyezzen a CraftingTable item nevével!
        {
            isBuildingHeld = true;
        }

        // 3. Mi van a földön?
        bool isFarmableLand = GameManager.Instance.tileManager.IsInteractable(targetPosition);
        bool isMineableOre = GameManager.Instance.tileManager.IsIronOre(targetPosition);
        bool isChoppableTree = GameManager.Instance.tileManager.IsTree(targetPosition);
        bool isWorkbench = GameManager.Instance.tileManager.IsWorkbench(targetPosition);

        // 4. HIGHLIGHT ÉS JELZÉS
        if (highlightSquare != null)
        {
            highlightSquare.transform.position = targetPosition + new Vector3(0.5f, 0.5f, 0f);
            highlightSquare.SetActive(isFarmableLand || isMineableOre || isChoppableTree || isBuildingHeld || isWorkbench);

            // Csak akkor próbáljuk színezni, ha tényleg találtunk SpriteRenderert!
            if (highlightSR != null)
            {
                if (isBuildingHeld)
                {
                    bool canPlace = GameManager.Instance.tileManager.CanPlaceBuilding(targetPosition);
                    highlightSR.color = canPlace ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
                }
                else
                {
                    highlightSR.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }

        // 5. Interakció (E gomb)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager.Instance.tileManager.IsWorkbench(targetPosition))
            {
                WorkbenchUI.instance.ToggleUI(); // Ezt a scriptet mindjárt megírjuk!
                return; // Kilépünk, ne csináljon mást
            }
            if (isBuildingHeld)
            {
                if (GameManager.Instance.tileManager.CanPlaceBuilding(targetPosition))
                {
                    GameManager.Instance.tileManager.PlaceWorkbench(targetPosition);
                    InventoryManager.instance.GetSelectedItem(true); // Elhasználunk 1-et
                    Debug.Log("Letetted a munkaasztalt!");
                }
            }
            else if (isMineableOre)
            {
                if (selectedItem != null && selectedItem.name == "Pickaxe")
                    InventoryManager.instance.Additem(ironOreItem);
            }
            else if (isChoppableTree)
            {
                if (selectedItem != null && selectedItem.name == "Axe")
                    InventoryManager.instance.Additem(woodItem);
            }
            else if (isFarmableLand)
            {
                GameManager.Instance.tileManager.SetInteracted(targetPosition);
            }
        }
    }
}