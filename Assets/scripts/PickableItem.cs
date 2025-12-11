using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public Item item; // Ez tárolja, hogy "mi ez" (pl. vetőmag, kő)

    private void Start()
    {
        // Amikor létrejön a tárgy, beállítjuk a kinézetét az Item adatai alapján
        if (item != null)
        {
            GetComponent<SpriteRenderer>().sprite = item.image;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Megnézzük, hogy a "Player" ment-e bele
        if (collision.CompareTag("Player"))
        {
            // Megpróbáljuk berakni az Inventoryba
            bool sikeresFelvetel = InventoryManager.instance.Additem(item);

            // Ha sikerült (volt hely), akkor töröljük a földről
            if (sikeresFelvetel)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Nincs hely az inventoryban!");
            }
        }
    }
}