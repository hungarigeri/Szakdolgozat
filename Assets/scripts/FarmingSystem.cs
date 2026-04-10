using UnityEngine;

public class FarmingSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject highlightSquare;

    [Header("Item References")]
    public Item ironOreItem;
    public Item woodItem;

    [Header("Prefabok")]
    public GameObject miningDrillPrefab;
    public GameObject barrelPrefab;

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

        // JAVÍTVA: Hozzáadtuk a MiningDrill-t is a listához!
        if (selectedItem != null && (selectedItem.name == "CraftingTable" || selectedItem.name == "Furnace" || selectedItem.name == "MiningDrill" || selectedItem.name == "Barrel"))
        {
            isBuildingHeld = true;
        }

        // 3. Mi van a földön?
        bool isFarmableLand = GameManager.Instance.tileManager.IsInteractable(targetPosition);
        bool isMineableOre = GameManager.Instance.tileManager.IsIronOre(targetPosition);
        bool isChoppableTree = GameManager.Instance.tileManager.IsTree(targetPosition);
        bool isWorkbench = GameManager.Instance.tileManager.IsWorkbench(targetPosition);
        bool isFurnace = GameManager.Instance.tileManager.IsFurnace(targetPosition);

        // ÚJ: Megnézzük, van-e ott Hordó vagy Fúró (Prefab)
        bool hasPrefab = HasPrefabBuilding(targetPosition);

        // 4. HIGHLIGHT ÉS JELZÉS
        if (highlightSquare != null)
        {
            highlightSquare.transform.position = targetPosition + new Vector3(0.5f, 0.5f, 0f);

            // JAVÍTÁS: A hasPrefab-ot és az isFurnace-t is hozzáadtuk, hogy ezeken is megjelenjen a keret!
            highlightSquare.SetActive(isFarmableLand || isMineableOre || isChoppableTree || isBuildingHeld || isWorkbench || isFurnace || hasPrefab);

            if (highlightSR != null)
            {
                if (isBuildingHeld)
                {
                    bool canPlace = false;

                    // Különleges szabály a Fúróra
                    if (selectedItem != null && selectedItem.name == "MiningDrill")
                    {
                        // Csak akkor zöld, ha érc van ott, ÉS nincs még rajta semmilyen prefab
                        canPlace = isMineableOre && !hasPrefab;
                    }
                    // Minden más normál épület szabálya (beleértve a hordót is)
                    else
                    {
                        // A TileManager megnézi, hogy a fű üres-e, MI pedig megnézzük, hogy nincs-e ott egy Prefab!
                        canPlace = GameManager.Instance.tileManager.CanPlaceBuilding(targetPosition) && !hasPrefab;
                    }

                    // Színezés zöldre vagy pirosra
                    highlightSR.color = canPlace ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
                }
                else
                {
                    // Sima fehér kijelölő interakcióhoz
                    highlightSR.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }

        // 5. Interakció (E gomb)
        if (Input.GetKeyDown(KeyCode.E))
        {
            // MUNKAASZTAL MEGNYITÁSA
            if (GameManager.Instance.tileManager.IsWorkbench(targetPosition))
            {
                WorkbenchUI.instance.ToggleUI();
                return;
            }

            // KEMENCE MEGNYITÁSA
            if (GameManager.Instance.tileManager.IsFurnace(targetPosition))
            {
                FurnaceUI.instance.ToggleUI();
                return;
            }

            // FÚRÓ TARTALMÁNAK KIVÉTELE
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f));
            if (hit != null && hit.GetComponent<MiningDrill>() != null)
            {
                hit.GetComponent<MiningDrill>().CollectItems();
                return;
            }

            // HORDÓ / LÁDA MEGNYITÁSA
            if (hit != null && hit.GetComponent<StorageContainer>() != null)
            {
                StorageContainer clickedStorage = hit.GetComponent<StorageContainer>();
                StorageUI.instance.OpenStorage(clickedStorage); // Ezt a scriptet most írjuk meg!
                return;
            }

            // --- JAVÍTOTT ÉPÍTÉSI LOGIKA ---
            if (isBuildingHeld)
            {
                // KÜLÖNLEGES ESET: Fúró lerakása (CSAK ércre rakható)
                if (selectedItem.name == "MiningDrill")
                {
                    if (isMineableOre) // Itt direkt azt nézzük, hogy VAN-E alatta vas!
                    {
                        Instantiate(miningDrillPrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.identity);
                        InventoryManager.instance.GetSelectedItem(true); // Elhasznál 1-et
                        Debug.Log("Fúró sikeresen lerakva az ércre!");
                    }
                    else
                    {
                        Debug.Log("A fúrót csak vasércre lehet lerakni!");
                    }
                }
                // NORMÁL ESET: Minden más épület (CSAK üres helyre rakható)
                else
                {
                    if (GameManager.Instance.tileManager.CanPlaceBuilding(targetPosition))
                    {
                        // Ha Hordó, akkor PREFAB-ként rakjuk le a rácsra
                        if (selectedItem.name == "Barrel")
                        {
                            Instantiate(barrelPrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.identity);
                            InventoryManager.instance.GetSelectedItem(true);
                            Debug.Log("Leraktad a hordó prefabot!");
                        }
                        // Ha Munkaasztal vagy Kemence, akkor TILE-ként rakjuk le
                        else
                        {
                            GameManager.Instance.tileManager.PlaceBuilding(targetPosition, selectedItem.name);
                            InventoryManager.instance.GetSelectedItem(true);
                            Debug.Log("Letetted a következőt: " + selectedItem.name);
                        }
                    }
                }
            }
            // BÁNYÁSZAT CSÁKÁNNYAL (Ha nincs fúró a kezünkben)
            else if (isMineableOre)
            {
                if (selectedItem != null && selectedItem.name == "Pickaxe")
                    InventoryManager.instance.Additem(ironOreItem);
            }
            // FAVÁGÁS
            else if (isChoppableTree)
            {
                if (selectedItem != null && selectedItem.name == "Axe")
                    InventoryManager.instance.Additem(woodItem);
            }
            // FÖLDMŰVELÉS
            else if (isFarmableLand)
            {
                GameManager.Instance.tileManager.SetInteracted(targetPosition);
            }
        }
    }

    // --- SEGÉDFÜGGVÉNY: Van-e Prefab ezen a kockán? ---
    private bool HasPrefabBuilding(Vector3Int position)
    {
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(position.x + 0.5f, position.y + 0.5f));
        if (hit != null)
        {
            // Ha fúró vagy hordó van alatta, akkor foglalt!
            if (hit.GetComponent<MiningDrill>() != null || hit.GetComponent<StorageContainer>() != null)
            {
                return true;
            }
        }
        return false;
    }
}