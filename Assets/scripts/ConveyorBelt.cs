using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Grafikák")]
    public Sprite straightSprite; // Az egyenes szalag képe
    public Sprite cornerSprite;   // A kanyarodó szalag képe
    private SpriteRenderer sr;

    [Header("Beállítások")]
    public float speed = 1f;
    [Header("Állapot")]
    public Item currentItem;
    private float progress = 0f;

    private SpriteRenderer itemVisual;
    private Vector2 direction;
    private Vector3 incomingLocalPos = new Vector3(0, 5f, 0);

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

        UpdateShape();
    }

    void Update()
    {
        if (currentItem != null)
        {
            progress += Time.deltaTime * speed;

            // --- BÉZIER-GÖRBE MATEMATIKA ---
            Vector3 p0 = incomingLocalPos;          // 1. Honnan jön (A belépési pont)
            Vector3 p1 = Vector3.zero;              // 2. A csempe közepe (Ezen törik meg az ív)
            Vector3 p2 = new Vector3(0, -5f, 0);  // 3. Hova megy (Mindig lefelé hagyja el a szalagot)

            // Kiszámoljuk az ívet a 3 pont között:
            Vector3 m1 = Vector3.Lerp(p0, p1, progress);
            Vector3 m2 = Vector3.Lerp(p1, p2, progress);
            itemVisual.transform.localPosition = Vector3.Lerp(m1, m2, progress);
            // --------------------------------

            if (progress >= 1f)
            {
                TryPassItem();
            }
        }
    }

    // Ezt a függvényt akkor hívjuk meg, amikor leraksz egy szalagot a FarmingSystem-ben
    public void UpdateShape()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        bool hasInputFromBehind = false;
        Vector2 sideInputDirection = Vector2.zero;
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir;
            Collider2D hit = Physics2D.OverlapPoint(checkPos);

            if (hit != null)
            {
                ConveyorBelt neighborBelt = hit.GetComponent<ConveyorBelt>();
                if (neighborBelt != null && neighborBelt.direction == -dir)
                {
                    if (neighborBelt.direction == this.direction) hasInputFromBehind = true;
                    else sideInputDirection = neighborBelt.direction;
                }
            }
        }

        if (hasInputFromBehind)
        {
            sr.sprite = straightSprite;
            sr.flipX = false;
            incomingLocalPos = new Vector3(0, 5f, 0); // Egyenes (Fentről jön)
        }
        else if (sideInputDirection != Vector2.zero)
        {
            sr.sprite = cornerSprite;
            float cross = (sideInputDirection.x * this.direction.y) - (sideInputDirection.y * this.direction.x);

            if (cross > 0)
            {
                sr.flipX = false;
                incomingLocalPos = new Vector3(5f, 0, 0); // Jobbról jön be a kanyarba
            }
            else
            {
                sr.flipX = true;
                incomingLocalPos = new Vector3(-5f, 0, 0); // Balról jön be a kanyarba
            }
        }
        else
        {
            sr.sprite = straightSprite;
            sr.flipX = false;
            incomingLocalPos = new Vector3(0, 5f, 0); // Alap (Egyenes)
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