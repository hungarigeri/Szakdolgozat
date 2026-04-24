using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Tilemaps")]

    public Tilemap oreMap;          // Ércek (Vas)
    public Tilemap treeMap;         // ÚJ: Fák (Favágás)
    public Tilemap buildingMap;
    // Épületek (pl. Munkaasztal)

    [Header("Tiles")]
    public Tile hiddenInteractableTile;
    public Tile InterectedTile;
    public Tile munkaasztalTile;
    public Tile furnaceTile;
    public TileBase tilledTile;
    public Tilemap grassMap;
    public TileBase grassTile;
    public float soilDecayTime = 60f; // Hány másodperc után tűnjön el a megművelt föld (Pl. 60 mp)

    // Ez a "láthatatlan lista", ami megjegyzi: [Melyik csempe] -> [Mikor lett felásva]
    private Dictionary<Vector3Int, float> tilledTilesTracker = new Dictionary<Vector3Int, float>();

    [Header("Mezőgazdasági beállítások")]
    public Item seedItem; // Ide is húzd be a magot!
    public float digSeedDropChance = 10f; // Alapból 10% esély

    void Start()
    {

    }

    void Update()
    {
        // Csinálunk egy ideiglenes listát azokról, amiknek lejárt az ideje
        List<Vector3Int> expiredTiles = new List<Vector3Int>();

        // 1. Végignézzük az összes felásott földet
        foreach (var kvp in tilledTilesTracker)
        {
            // kvp.Key = A koordináta, kvp.Value = Amikor felástuk
            // Ha a mostani időből kivonva az ásás idejét, több telt el, mint a megengedett (pl. 60mp):
            if (Time.time - kvp.Value >= soilDecayTime)
            {
                expiredTiles.Add(kvp.Key);
            }
        }

        // 2. Végigmegyünk a lejárt földeken
        foreach (Vector3Int pos in expiredTiles)
        {
            // Megnézzük, VETETTEK-E BELE NÖVÉNYT? (Van-e valami a közepén)
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(pos.x + 0.5f, pos.y + 0.5f));

            if (hit != null && hit.GetComponent<CropGrowth>() != null)
            {
                // HA VAN BENNE NÖVÉNY: Nem tüntetjük el a földet! 
                // Újraindítjuk az időzítőt, hogy majd aratás után kezdjen el újra ketyegni.
                tilledTilesTracker[pos] = Time.time;
            }
            else
            {
                // HA ÜRES: Visszanő a fű!
                grassMap.SetTile(pos, grassTile); // Visszatesszük a füvet
                tilledTilesTracker.Remove(pos);   // Levesszük a figyelőlistáról
            }
        }
    }

    public bool IsInteractable(Vector3Int position)
    {
        // Ha van BÁRMILYEN csempe a "fű" rétegen ezen a koordinátán, akkor igaz (lehet ásni)!
        return grassMap.HasTile(position);
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

    // Leteszi a a buldinget a térképre
    public void PlaceBuilding(Vector3Int position, string buildingName)
    {
        if (buildingMap == null) return;

        if (buildingName == "CraftingTable")
        {
            buildingMap.SetTile(position, munkaasztalTile);
        }
        else if (buildingName == "Furnace") // ÚJ: Kemence lerakása
        {
            buildingMap.SetTile(position, furnaceTile);
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

    public bool IsFurnace(Vector3Int position)
    {
        if (buildingMap == null) return false;
        TileBase tile = buildingMap.GetTile(position);
        // Ide írd a kemence tile pontos nevét (pl. furnace_0)
        return tile != null && (tile.name == "furnice_0" || tile.name == "Furnice_0");
    }

    // Ez a TileManager-ben legyen
    public void TillGround(Vector3Int position)
    {
        if (IsInteractable(position))
        {
            grassMap.SetTile(position, tilledTile);
            tilledTilesTracker[position] = Time.time;

            // VÉLETLEN: Kifordul-e egy mag a földből?
            if (Random.Range(0f, 100f) <= digSeedDropChance && seedItem != null)
            {
                InventoryManager.instance.Additem(seedItem);
            }
        }
    }
    // Ezt keresi a FarmingSystem, hogy tudja, vethet-e magot!
    public bool IsTilled(Vector3Int position)
    {
        TileBase tile = grassMap.GetTile(position);

        // Ha a fű rétegen lévő csempe megegyezik a sár csempével
        if (tile == tilledTile)
        {
            return true;
        }
        return false;
    }
}


