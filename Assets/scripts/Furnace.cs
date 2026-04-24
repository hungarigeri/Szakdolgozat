using UnityEngine;

public class Furnace : MonoBehaviour
{
    [Header("Típus")]
    public bool isAutomated = false;

    [Header("Állapot (Ezt olvassa a UI)")]
    public CraftingRecipe currentRecipe; // <-- Ez a sor oldja meg a piros hibát!
    public int storedInput = 0;
    public int storedOutput = 0;
    public float currentProgress = 0f;
    public float smeltSpeed = 0.5f;

    void Update()
    {
        // 1. SÜTÉS LOGIKA
        if (currentRecipe != null && storedInput > 0)
        {
            currentProgress += Time.deltaTime * smeltSpeed;

            if (currentProgress >= currentRecipe.requiredWork)
            {
                storedInput--;
                storedOutput++;
                currentProgress = 0f;
            }
        }
        else
        {
            currentProgress = 0f;
        }


        // 2. AUTOMATA KIMENET
        if (isAutomated && storedOutput > 0)
        {
            TryOutputItem();
        }
    }

    // --- OLDALSÓ BEMENET (A Bal oldalról várja az anyagot) ---
    public bool AcceptItem(Item item, Vector3 sourcePosition)
    {
        if (!isAutomated || currentRecipe == null) return false;

        if (item != currentRecipe.ingredients[0].item) return false;

        // Kiszámoljuk a "Bal oldalt" (-transform.right)
        Vector3 expectedInputPos = transform.position - transform.right;

        // Csak akkor fogadja el, ha a szalag pont a bal oldalán van!
        if (Vector2.Distance(sourcePosition, expectedInputPos) > 0.1f)
        {
            return false;
        }

        storedInput++;
        return true;
    }

    // --- OLDALSÓ KIMENET (A Jobb oldalra köpi ki az anyagot) ---
    void TryOutputItem()
    {
        // Kiszámoljuk a "Jobb oldalt" (+transform.right)
        Vector2 checkDirection = transform.right;
        Vector2 targetPos = (Vector2)transform.position + checkDirection;

        Collider2D hit = Physics2D.OverlapPoint(targetPos);

        if (hit != null)
        {
            ConveyorBelt belt = hit.GetComponent<ConveyorBelt>();
            if (belt != null && belt.AcceptItem(currentRecipe.resultItem))
            {
                storedOutput--;
            }
        }
    }

    // --- GIZMOS (Nyilak rajzolása a Bal/Jobb oldalra) ---
    private void OnDrawGizmos()
    {
        if (isAutomated)
        {
            // KIMENET (Sárga nyíl Jobbra)
            Gizmos.color = Color.yellow;
            Vector3 outStart = transform.position;
            Vector3 outEnd = transform.position + transform.right * 0.8f;
            Gizmos.DrawLine(outStart, outEnd);

            Vector3 rightOut = Quaternion.Euler(0, 0, 145) * transform.right * 0.25f;
            Vector3 leftOut = Quaternion.Euler(0, 0, -145) * transform.right * 0.25f;
            Gizmos.DrawLine(outEnd, outEnd + rightOut);
            Gizmos.DrawLine(outEnd, outEnd + leftOut);

            // BEMENET (Kék nyíl Balról)
            Gizmos.color = Color.cyan;
            Vector3 inStart = transform.position - transform.right * 0.8f;
            Vector3 inEnd = transform.position;
            Gizmos.DrawLine(inStart, inEnd);

            Vector3 rightIn = Quaternion.Euler(0, 0, 145) * transform.right * 0.25f;
            Vector3 leftIn = Quaternion.Euler(0, 0, -145) * transform.right * 0.25f;
            Gizmos.DrawLine(inEnd, inEnd + rightIn);
            Gizmos.DrawLine(inEnd, inEnd + leftIn);
        }
    }
}