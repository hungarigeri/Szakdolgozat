using UnityEngine;
using TMPro;

public class MiningDrill : MonoBehaviour
{
    [Header("Típus Beállítások")]
    public bool canAutoOutput = false;

    [Header("Beállítások")]
    public float miningInterval = 3f;
    public float outputInterval = 1f;
    public int maxCapacity = 64;
    public Item oreToYield;

    [Header("Állapot")]
    public int storedAmount = 0;
    private float timer;
    private float outputTimer;
    private Vector3Int gridPosition;

    [Header("UI")]
    public TextMeshPro floatingText;

    void Start()
    {
        gridPosition = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), 0);
        timer = miningInterval;
        outputTimer = outputInterval;
        UpdateUI();
    }

    void Update()
    {
        if (storedAmount < maxCapacity && GameManager.Instance.tileManager != null && GameManager.Instance.tileManager.IsIronOre(gridPosition))
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Mine();
                timer = miningInterval;
            }
        }

        if (canAutoOutput && storedAmount > 0)
        {
            outputTimer -= Time.deltaTime;
            if (outputTimer <= 0)
            {
                TryOutputItem();
                outputTimer = outputInterval;
            }
        }
    }

    void Mine()
    {
        storedAmount++;
        UpdateUI();
    }

    public void CollectItems()
    {
        if (storedAmount > 0)
        {
            for (int i = 0; i < storedAmount; i++)
            {
                InventoryManager.instance.Additem(oreToYield);
            }
            storedAmount = 0;
            UpdateUI();
        }
    }

    void TryOutputItem()
    {
        // Kiszámoljuk hova néz a fúró (a transform.right-ot használjuk, mert a rajzod alapján az az eleje)
        Vector2 targetPos = (Vector2)transform.position + (Vector2)transform.right;

        Debug.DrawLine(transform.position, targetPos, Color.red, 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 0.2f);
        bool successfullyPassed = false;

        foreach (Collider2D hit in hits)
        {
            ConveyorBelt belt = hit.GetComponent<ConveyorBelt>();
            if (belt != null)
            {
                if (oreToYield != null && belt.AcceptItem(oreToYield))
                {
                    successfullyPassed = true;
                    break;
                }
            }

            StorageContainer storage = hit.GetComponent<StorageContainer>();
            if (storage != null)
            {
                if (oreToYield != null)
                {
                    storage.DepositItem(oreToYield, 1);
                    successfullyPassed = true;
                    break;
                }
            }
        }

        // --- ÚJ: HA NINCS ELŐTTE SEMMI, A FÖLDRE KÖPI ---
        if (!successfullyPassed && oreToYield != null)
        {
            InventoryManager.instance.SpawnItemInWorld(oreToYield, new Vector3(targetPos.x, targetPos.y, 0));
            storedAmount--;
            UpdateUI();
        }
        else if (successfullyPassed)
        {
            storedAmount--;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (floatingText != null)
        {
            floatingText.text = $"{storedAmount} / {maxCapacity}\n<size=50%>{miningInterval} mp/db</size>";
            floatingText.color = (storedAmount >= maxCapacity) ? Color.red : Color.white;
        }
    }

    private void OnDrawGizmos()
    {
        if (!canAutoOutput) return;
        Gizmos.color = Color.yellow;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + transform.right * 0.6f;
        Gizmos.DrawLine(startPos, endPos);
        Vector3 arrowTip1 = endPos - transform.right * 0.2f + transform.up * 0.2f;
        Vector3 arrowTip2 = endPos - transform.right * 0.2f - transform.up * 0.2f;
        Gizmos.DrawLine(endPos, arrowTip1);
        Gizmos.DrawLine(endPos, arrowTip2);
    }
}