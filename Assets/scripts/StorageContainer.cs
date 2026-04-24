using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StoredItem
{
    public Item item;
    public int count;
}

public class StorageContainer : MonoBehaviour
{
    [Header("Automata Kimenet")]
    public bool autoOutput = true;
    public float outputSpeed = 1f;
    private float outputTimer = 0f;

    public List<StoredItem> contents = new List<StoredItem>();

    // Beteszünk egy tárgyat
    public void DepositItem(Item itemToSave, int amount)
    {
        StoredItem existing = contents.Find(x => x.item == itemToSave);
        if (existing != null)
        {
            existing.count += amount;
        }
        else
        {
            StoredItem newItem = new StoredItem();
            newItem.item = itemToSave;
            newItem.count = amount;
            contents.Add(newItem);
        }
    }

    // Kiveszünk egy tárgyat
    public void WithdrawItem(Item itemToTake, int amount)
    {
        StoredItem existing = contents.Find(x => x.item == itemToTake);
        if (existing != null)
        {
            existing.count -= amount;
            if (existing.count <= 0)
            {
                contents.Remove(existing);
            }
        }
    }

    void Update()
    {
        // ÚJ: Megnézzük, hogy be van-e kapcsolva az ürítés, és van-e BÁRMI a listában (Count > 0)
        if (autoOutput && contents.Count > 0)
        {
            outputTimer += Time.deltaTime;

            if (outputTimer >= outputSpeed)
            {
                TryOutputToBelt();
                outputTimer = 0f;
            }
        }
    }

    void TryOutputToBelt()
    {
        Vector2 targetPos = (Vector2)transform.position + (Vector2)transform.right;
        Collider2D hit = Physics2D.OverlapPoint(targetPos);

        if (hit != null)
        {
            ConveyorBelt belt = hit.GetComponent<ConveyorBelt>();

            // Ha van szalag ÉS van valami a hordóban
            if (belt != null && contents.Count > 0)
            {
                // ÚJ: Mindig a lista legelső elemét (0. index) próbáljuk meg kipakolni
                StoredItem itemToOutput = contents[0];

                // Csak akkor vonjuk le, ha a szalag tényleg el is fogadta!
                if (belt.AcceptItem(itemToOutput.item))
                {
                    itemToOutput.count--; // Levonunk egyet

                    // Ha ebből a tárgyból kifogytunk, kitöröljük a listából
                    if (itemToOutput.count <= 0)
                    {
                        contents.RemoveAt(0);
                    }

                    // UI frissítés
                    if (StorageUI.instance != null && StorageUI.instance.storagePanel.activeSelf)
                    {
                        StorageUI.instance.UpdateUI();
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (autoOutput)
        {
            Gizmos.color = Color.yellow;
            Vector3 outStart = transform.position;
            Vector3 outEnd = transform.position + transform.right * 0.8f;
            Gizmos.DrawLine(outStart, outEnd);
        }
    }
}