using UnityEngine;
using TMPro; // FONTOS: Ez kell a TextMeshPro-hoz!

public class MiningDrill : MonoBehaviour
{
    [Header("Beállítások")]
    public float miningInterval = 3f;
    public int maxCapacity = 64;
    public Item oreToYield;

    [Header("Állapot")]
    public int storedAmount = 0;
    private float timer;
    private Vector3Int gridPosition;

    [Header("UI")]
    public TextMeshPro floatingText; // ÚJ: Ide húzzuk be a szöveget!

    void Start()
    {
        gridPosition = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), 0);
        timer = miningInterval;

        UpdateUI(); // Induláskor rögtön kiírjuk a 0-s állapotot
    }

    void Update()
    {
        if (storedAmount >= maxCapacity) return;

        // Ellenőrizzük, van-e érc alatta
        if (GameManager.Instance.tileManager == null || !GameManager.Instance.tileManager.IsIronOre(gridPosition)) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Mine();
            timer = miningInterval;
        }
    }

    void Mine()
    {
        storedAmount++;
        UpdateUI(); // ÚJ: Frissítjük a szöveget, amint kapunk egy ércet
    }

    public void CollectItems()
    {
        if (storedAmount > 0)
        {
            for (int i = 0; i < storedAmount; i++)
            {
                InventoryManager.instance.Additem(oreToYield);
            }
            Debug.Log("Kivettél " + storedAmount + " ércet a fúróból.");
            storedAmount = 0;

            UpdateUI(); // ÚJ: Frissítjük a szöveget a kiürítés után
        }
    }

    // --- ÚJ FÜGGVÉNY A SZÖVEG MEGJELENÍTÉSÉRE ---
    void UpdateUI()
    {
        if (floatingText != null)
        {
            // Rich Text formázás: A felső sor az érc, alatta kisebb betűvel a sebesség
            floatingText.text = $"{storedAmount} / {maxCapacity}\n<size=50%>{miningInterval} mp/db</size>";

            // Vizuális extra: Ha megtelt a fúró, a szöveg pirosra vált!
            if (storedAmount >= maxCapacity)
            {
                floatingText.color = Color.red;
            }
            else
            {
                floatingText.color = Color.white;
            }
        }
    }
}