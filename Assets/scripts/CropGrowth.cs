using UnityEngine;

public class CropGrowth : MonoBehaviour
{
    [Header("Grafika")]
    public Sprite grownSprite; 
    private SpriteRenderer sr;

    [Header("Aratás Beállításai")]
    public Item harvestedItem; // Mit ad a növény? (Pl. Búza Item)
    public int yieldAmount = 1; // Mennyit adjon?

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
            // Odaadja a játékosnak a kész terményt
            if (harvestedItem != null)
            {
                InventoryManager.instance.Additem(harvestedItem);
                // Ha többet akarsz adni: (Ide írhatsz egy for ciklust, ha yieldAmount > 1)
            }

            // Eltűnik a növény a földről
            Destroy(gameObject);
        }
    }
}