using UnityEngine;

public class FarmingSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject highlightSquare;
    public GameObject directionArrow; // Ezt használjuk a mutatónak!

    [Header("Item References")]
    public Item ironOreItem;
    public Item woodItem;

    [Header("Prefabok")]
    public GameObject miningDrillPrefab;
    public GameObject automataDrillPrefab;
    public GameObject barrelPrefab;
    public GameObject conveyorPrefab;

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

        if (selectedItem != null && (selectedItem.name == "CraftingTable" || selectedItem.name == "Furnace" || selectedItem.name == "MiningDrill" || selectedItem.name == "AutomataDrill" || selectedItem.name == "Barrel" || selectedItem.name == "ConveyorBelt"))
        {
            isBuildingHeld = true;
        }

        bool isFarmableLand = GameManager.Instance.tileManager.IsInteractable(targetPosition);
        bool isMineableOre = GameManager.Instance.tileManager.IsIronOre(targetPosition);
        bool isChoppableTree = GameManager.Instance.tileManager.IsTree(targetPosition);
        bool isWorkbench = GameManager.Instance.tileManager.IsWorkbench(targetPosition);
        bool isFurnace = GameManager.Instance.tileManager.IsFurnace(targetPosition);
        bool hasPrefab = HasPrefabBuilding(targetPosition);

        // --- HIGHLIGHT ÉS JELZÉS ---
        if (highlightSquare != null)
        {
            highlightSquare.transform.position = targetPosition + new Vector3(0.5f, 0.5f, 0f);

            // JAVÍTVA: DINAMIKUS NYÍL FORGATÁS!
            if (selectedItem != null && selectedItem.name == "ConveyorBelt")
            {
                highlightSquare.transform.rotation = Quaternion.Euler(0, 0, currentPlacementRotation);
                if (directionArrow != null)
                {
                    directionArrow.SetActive(true);
                    // A Szalag alapból LEFELÉ (-Y) megy, a nyilat ehhez igazítjuk (180 fok)
                    directionArrow.transform.localRotation = Quaternion.Euler(0, 0, 180f);
                }
            }
            else if (selectedItem != null && selectedItem.name == "AutomataDrill")
            {
                highlightSquare.transform.rotation = Quaternion.Euler(0, 0, currentPlacementRotation);
                if (directionArrow != null)
                {
                    directionArrow.SetActive(true);
                    // A Fúró alapból JOBBRA (+X) lő, a nyilat ehhez igazítjuk (-90 fok)
                    directionArrow.transform.localRotation = Quaternion.Euler(0, 0, -90f);
                }
            }
            else
            {
                highlightSquare.transform.rotation = Quaternion.identity;
                if (directionArrow != null) directionArrow.SetActive(false);
            }

            highlightSquare.SetActive(isFarmableLand || isMineableOre || isChoppableTree || isBuildingHeld || isWorkbench || isFurnace || hasPrefab);

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

        // --- LERAKÁS LOGIKA (E GOMB) ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager.Instance.tileManager.IsWorkbench(targetPosition)) { WorkbenchUI.instance.ToggleUI(); return; }
            if (GameManager.Instance.tileManager.IsFurnace(targetPosition)) { FurnaceUI.instance.ToggleUI(); return; }

            Collider2D hit = Physics2D.OverlapPoint(new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f));
            if (hit != null && hit.GetComponent<MiningDrill>() != null) { hit.GetComponent<MiningDrill>().CollectItems(); return; }
            if (hit != null && hit.GetComponent<StorageContainer>() != null) { StorageUI.instance.OpenStorage(hit.GetComponent<StorageContainer>()); return; }

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
            if (hit.GetComponent<MiningDrill>() != null || hit.GetComponent<StorageContainer>() != null || hit.GetComponent<ConveyorBelt>() != null) return true;
        }
        return false;
    }
}