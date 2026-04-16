using UnityEngine;

public class FarmingSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject highlightSquare;
    public GameObject directionArrow;

    [Header("Item References")]
    public Item ironOreItem;
    public Item woodItem;

    [Header("Prefabok")]
    public GameObject miningDrillPrefab;
    public GameObject automataDrillPrefab;
    public GameObject barrelPrefab;
    public GameObject conveyorPrefab;

    // --- JAVÍTÁS: Ketté választottuk a két kemence prefabot! ---
    public GameObject basicFurnacePrefab;
    public GameObject automataFurnacePrefab;

    private float currentPlacementRotation = 0f;
    private Vector2 lastFacingDirection = Vector2.down;
    private SpriteRenderer highlightSR;

    void Start()
    {
        if (highlightSquare != null) highlightSR = highlightSquare.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (InventoryManager.instance == null || GameManager.Instance == null || GameManager.Instance.tileManager == null) return;

        // --- FORGATÁS ---
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentPlacementRotation -= 90f;
            if (currentPlacementRotation <= -360f) currentPlacementRotation = 0f;
        }

        // --- IRÁNYÍTÁS ---
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

        Item selectedItem = InventoryManager.instance.GetSelectedItem(false);
        bool isBuildingHeld = false;

        if (selectedItem != null && (selectedItem.name == "CraftingTable" || selectedItem.name == "Furnace" || selectedItem.name == "AutomataFurnace" || selectedItem.name == "Automata Furnace" || selectedItem.name == "MiningDrill" || selectedItem.name == "AutomataDrill" || selectedItem.name == "Barrel" || selectedItem.name == "ConveyorBelt"))
        {
            isBuildingHeld = true;
        }

        bool isFarmableLand = GameManager.Instance.tileManager.IsInteractable(targetPosition);
        bool isMineableOre = GameManager.Instance.tileManager.IsIronOre(targetPosition);
        bool isChoppableTree = GameManager.Instance.tileManager.IsTree(targetPosition);
        bool isWorkbench = GameManager.Instance.tileManager.IsWorkbench(targetPosition);
        bool isFurnaceTile = GameManager.Instance.tileManager.IsFurnace(targetPosition);
        bool hasPrefab = HasPrefabBuilding(targetPosition);

        // --- HIGHLIGHT ÉS JELZÉS ---
        if (highlightSquare != null)
        {
            highlightSquare.transform.position = targetPosition + new Vector3(0.5f, 0.5f, 0f);

            if (selectedItem != null && selectedItem.name == "ConveyorBelt")
            {
                highlightSquare.transform.rotation = Quaternion.Euler(0, 0, currentPlacementRotation);
                if (directionArrow != null)
                {
                    directionArrow.SetActive(true);
                    directionArrow.transform.localRotation = Quaternion.Euler(0, 0, 180f);
                }
            }
            else if (selectedItem != null && selectedItem.name == "AutomataDrill")
            {
                highlightSquare.transform.rotation = Quaternion.Euler(0, 0, currentPlacementRotation);
                if (directionArrow != null)
                {
                    directionArrow.SetActive(true);
                    directionArrow.transform.localRotation = Quaternion.Euler(0, 0, -90f);
                }
            }
            // --- KEMENCE NYÍL (Csak az Automatának kell!) ---
            else if (selectedItem != null && (selectedItem.name == "AutomataFurnace" || selectedItem.name == "Automata Furnace"))
            {
                highlightSquare.transform.rotation = Quaternion.Euler(0, 0, currentPlacementRotation);
                if (directionArrow != null)
                {
                    directionArrow.SetActive(true);

                    // --- MÓDOSÍTÁS: Output irány ---
                    // Mivel az Output most már a Jobb oldalon van (transform.right),
                    // a nyilat úgy forgatjuk, hogy jobbra mutasson (-90 fok).
                    // (Ez pontosan megegyezik a fúró kilövési irányával!)
                    directionArrow.transform.localRotation = Quaternion.Euler(0, 0, -90f);
                }
            }
            // Minden más (pl. sima Furnace, Hordó, CraftingTable) nyíl nélkül:
            else
            {
                highlightSquare.transform.rotation = Quaternion.identity;
                if (directionArrow != null) directionArrow.SetActive(false);
            }

            highlightSquare.SetActive(isFarmableLand || isMineableOre || isChoppableTree || isBuildingHeld || isWorkbench || isFurnaceTile || hasPrefab);

            if (highlightSR != null)
            {
                if (isBuildingHeld)
                {
                    bool canPlace = false;
                    if (selectedItem != null && (selectedItem.name == "MiningDrill" || selectedItem.name == "AutomataDrill"))
                        canPlace = isMineableOre && !hasPrefab;
                    else
                        canPlace = GameManager.Instance.tileManager.CanPlaceBuilding(targetPosition) && !hasPrefab && !isChoppableTree && !isMineableOre;

                    highlightSR.color = canPlace ? new Color(0f, 1f, 0f, 0.6f) : new Color(1f, 0f, 0f, 0.6f);
                }
                else
                {
                    highlightSR.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }

        // --- LERAKÁS ÉS INTERAKCIÓ LOGIKA (E GOMB) ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager.Instance.tileManager.IsWorkbench(targetPosition)) { WorkbenchUI.instance.ToggleUI(); return; }
            if (GameManager.Instance.tileManager.IsFurnace(targetPosition)) { FurnaceUI.instance.ToggleUI(); return; }

            Collider2D hit = Physics2D.OverlapPoint(new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f));
            if (hit != null)
            {
                if (hit.GetComponent<MiningDrill>() != null) { hit.GetComponent<MiningDrill>().CollectItems(); return; }
                if (hit.GetComponent<StorageContainer>() != null) { StorageUI.instance.OpenStorage(hit.GetComponent<StorageContainer>()); return; }

                Furnace hitFurnace = hit.GetComponent<Furnace>();
                if (hitFurnace != null) { FurnaceUI.instance.OpenUIForFurnace(hitFurnace); return; }
            }

            if (isBuildingHeld)
            {
                if (selectedItem.name == "MiningDrill")
                {
                    if (isMineableOre && !hasPrefab)
                    {
                        Instantiate(miningDrillPrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.identity);
                        InventoryManager.instance.GetSelectedItem(true);
                    }
                }
                else if (selectedItem.name == "AutomataDrill")
                {
                    if (isMineableOre && !hasPrefab)
                    {
                        Instantiate(automataDrillPrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.Euler(0, 0, currentPlacementRotation));
                        InventoryManager.instance.GetSelectedItem(true);
                    }
                }
                else if (GameManager.Instance.tileManager.CanPlaceBuilding(targetPosition) && !hasPrefab && !isChoppableTree && !isMineableOre)
                {
                    if (selectedItem.name == "ConveyorBelt")
                    {
                        Instantiate(conveyorPrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.Euler(0, 0, currentPlacementRotation));
                        InventoryManager.instance.GetSelectedItem(true);
                    }
                    else if (selectedItem.name == "Barrel")
                    {
                        Instantiate(barrelPrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.identity);
                        InventoryManager.instance.GetSelectedItem(true);
                    }
                    // --- JAVÍTÁS: Ketté választott kemence lerakás! ---
                    else if (selectedItem.name == "Furnace")
                    {
                        Instantiate(basicFurnacePrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.Euler(0, 0, currentPlacementRotation));
                        InventoryManager.instance.GetSelectedItem(true);
                    }
                    else if (selectedItem.name == "AutomataFurnace" || selectedItem.name == "Automata Furnace")
                    {
                        Instantiate(automataFurnacePrefab, new Vector3(targetPosition.x + 0.5f, targetPosition.y + 0.5f, 0), Quaternion.Euler(0, 0, currentPlacementRotation));
                        InventoryManager.instance.GetSelectedItem(true);
                    }
                    else
                    {
                        GameManager.Instance.tileManager.PlaceBuilding(targetPosition, selectedItem.name);
                        InventoryManager.instance.GetSelectedItem(true);
                    }
                }
            }
            else if (isMineableOre) { if (selectedItem != null && selectedItem.name == "Pickaxe") InventoryManager.instance.Additem(ironOreItem); }
            else if (isChoppableTree) { if (selectedItem != null && selectedItem.name == "Axe") InventoryManager.instance.Additem(woodItem); }
            else if (isFarmableLand) { GameManager.Instance.tileManager.SetInteracted(targetPosition); }
        }
    }

    private bool HasPrefabBuilding(Vector3Int position)
    {
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(position.x + 0.5f, position.y + 0.5f));
        if (hit != null)
        {
            if (hit.GetComponent<MiningDrill>() != null || hit.GetComponent<StorageContainer>() != null || hit.GetComponent<ConveyorBelt>() != null || hit.GetComponent<Furnace>() != null) return true;
        }
        return false;
    }
}