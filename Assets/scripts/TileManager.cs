using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap interactableMap; // Földművelés
    public Tilemap oreMap;          // Ércek (Vas)
    public Tilemap treeMap;         // ÚJ: Fák (Favágás)
    public Tilemap buildingMap;      // Épületek (pl. Munkaasztal)

    [Header("Tiles")]
    public Tile hiddenInteractableTile;
    public Tile InterectedTile;
    public Tile munkaasztalTile; // ÚJ: A munkaasztal csempéje

    void Start()
    {
        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(position);
            if (tile != null && tile.name == "Interactable_visible")
            {
                interactableMap.SetTile(position, hiddenInteractableTile);
            }
        }
    }

    public bool IsInteractable(Vector3Int position)
    {
        TileBase tileAtPosition = interactableMap.GetTile(position);
        if (tileAtPosition != null && tileAtPosition.name == "Interactable_Hidden") return true;
        return false;
    }

    public void SetInteracted(Vector3Int position)
    {
        interactableMap.SetTile(position, InterectedTile);
    }

    public bool IsIronOre(Vector3Int position)
    {
        if (oreMap == null) return false;
        TileBase tileAtPosition = oreMap.GetTile(position);
        if (tileAtPosition != null && (tileAtPosition.name == "iron_0" || tileAtPosition.name == "iron" || tileAtPosition.name == "iron_1")) return true;
        return false;
    }

    // --- ÚJ: FAVÁGÁS ---
    public bool IsTree(Vector3Int position)
    {
        if (treeMap == null) return false;

        TileBase tileAtPosition = treeMap.GetTile(position);

        // FONTOS: Ide a fa csempéid pontos nevét kell beírnod!
        // Például "tree", "tree_0", stb. Írd át arra, ahogy a Unity-ben elnevezted a fa képedet!
        if (tileAtPosition != null && (tileAtPosition.name == "fak_0" || tileAtPosition.name == "fak_1"))
        {
            return true;
        }
        return false;
    }

    // --- ÚJ: ÉPÍTÉS ---

    // Megnézi, hogy üres-e az adott kocka (nincs ott fa, érc vagy másik épület)
    public bool CanPlaceBuilding(Vector3Int position)
    {
        if (IsTree(position)) return false;
        if (IsIronOre(position)) return false;
        if (buildingMap != null && buildingMap.GetTile(position) != null) return false;

        return true; // Szabad a hely!
    }

    // Leteszi a Munkaasztalt a térképre
    public void PlaceWorkbench(Vector3Int position)
    {
        if (buildingMap != null && munkaasztalTile != null)
        {
            buildingMap.SetTile(position, munkaasztalTile);
        }
    }

    // Megnézi, hogy Munkaasztalra nézünk-e
    public bool IsWorkbench(Vector3Int position)
    {
        if (buildingMap == null) return false;

        TileBase tile = buildingMap.GetTile(position);

        // Zárójelbe tesszük a neveket, így a "tile != null" mindkettőre vonatkozik
        return tile != null && (tile.name == "Craftingtable_0" || tile.name == "craftingtable_0");
    }
}
