using UnityEngine;

public class CropGrowth : MonoBehaviour
{
    public Sprite grownSprite; // Inspector-ban húzd ide a felnőtt növény sprite-ot
    private SpriteRenderer sr;
    private bool isGrown = false;
    
    private bool playerInRange = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(Grow());
    }

    System.Collections.IEnumerator Grow()
    {
        yield return new WaitForSeconds(5f); // 5 másodperc növekedés
        sr.sprite = grownSprite; // Sprite váltás
        isGrown = true;
    }

   void Update()
    {
        // csak akkor reagáljon az F-re, ha a játékos közel van
        if (playerInRange && isGrown && Input.GetKeyDown(KeyCode.F))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}