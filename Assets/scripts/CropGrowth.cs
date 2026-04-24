using UnityEngine;

public class CropGrowth : MonoBehaviour
{
    [Header("Grafika")]
    public Sprite grownSprite;
    private SpriteRenderer sr;

    [Header("Aratás Beállításai")]
    public Item harvestedItem; // Mit ad a növény? (Pl. Búza Item)
    public int yieldAmount = 1; // Mennyit adjon?
    public Item seedItem;
    public float seedDropChance = 80f;
    // Public lett, hogy a FarmingSystem is lássa, megnőtt-e már!
    public bool isGrown = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(Grow());
    }

    System.Collections.IEnumerator Grow()
    {
        yield return new WaitForSeconds(5f); // 5 másodperc növekedés
        sr.sprite = grownSprite;
        isGrown = true;
    }

    // Ezt a függvényt a FarmingSystem hívja meg az 'E' gombbal!
    public void Harvest()
    {
        if (isGrown)
        {
            // 1. A termés (pl. búza) mindig jár
            if (harvestedItem != null) InventoryManager.instance.Additem(harvestedItem);

            // 2. VÉLETLEN: Kapunk-e magot is?
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= seedDropChance && seedItem != null)
            {
                InventoryManager.instance.Additem(seedItem);
                Debug.Log("Szerencse! Kaptál egy magot is.");
            }

            Destroy(gameObject);
        }
    }
}