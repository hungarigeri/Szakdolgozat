using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Beállítások")]
    public float speed = 1f;

    [Header("Állapot")]
    public Item currentItem;
    private float progress = 0f;

    private SpriteRenderer itemVisual;
    private Vector2 direction;

    void Start()
    {
        direction = -transform.up; // A grafikád alapján lefelé megy a szalag

        GameObject visualObj = new GameObject("ItemVisual");
        visualObj.transform.SetParent(transform);
        itemVisual = visualObj.AddComponent<SpriteRenderer>();
        itemVisual.sortingOrder = 4;
        itemVisual.transform.localPosition = Vector3.zero;
        itemVisual.transform.localScale = new Vector3(10f, 10f, 1f);
        visualObj.SetActive(false);
    }

    void Update()
    {
        if (currentItem != null)
        {
            // Haladás számítása
            progress += Time.deltaTime * speed;

            // JAVÍTÁS: Kicsit túllógatjuk a határokon, hogy ne legyen hézag
            // Fentről (0.5) indul és lemegy egészen a széléig (-0.5)
            float yPos = Mathf.Lerp(5f, -5f, progress);
            itemVisual.transform.localPosition = new Vector3(0, yPos, 0);

            if (progress >= 1f)
            {
                TryPassItem();
            }
        }
    }

    public bool AcceptItem(Item item)
    {
        if (currentItem == null)
        {
            currentItem = item;
            progress = 0f;
            itemVisual.sprite = item.image;
            itemVisual.gameObject.SetActive(true);
            return true;
        }
        return false;
    }

    void TryPassItem()
    {
        Vector2 targetPos = (Vector2)transform.position + direction;
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 0.2f);
        bool successfullyPassed = false;

        foreach (Collider2D hit in hits)
        {
            ConveyorBelt nextBelt = hit.GetComponent<ConveyorBelt>();
            if (nextBelt != null)
            {
                if (nextBelt.AcceptItem(currentItem))
                {
                    successfullyPassed = true;
                    ClearBelt();
                    return;
                }
            }

            StorageContainer storage = hit.GetComponent<StorageContainer>();
            if (storage != null)
            {
                storage.DepositItem(currentItem, 1);
                successfullyPassed = true;
                ClearBelt();
                return;
            }
        }

        // --- ÚJ: HA A SZALAG VÉGÉN NINCS SEMMI, LEESIK A FÖLDRE ---
        if (!successfullyPassed)
        {
            InventoryManager.instance.SpawnItemInWorld(currentItem, new Vector3(targetPos.x, targetPos.y, 0));
            ClearBelt();
        }
    }

    void ClearBelt()
    {
        currentItem = null;
        itemVisual.gameObject.SetActive(false);
        progress = 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - transform.up * 0.6f;
        Gizmos.DrawLine(startPos, endPos);
        Vector3 arrowTip1 = endPos + transform.up * 0.2f + transform.right * 0.2f;
        Vector3 arrowTip2 = endPos + transform.up * 0.2f - transform.right * 0.2f;
        Gizmos.DrawLine(endPos, arrowTip1);
        Gizmos.DrawLine(endPos, arrowTip2);
    }
}