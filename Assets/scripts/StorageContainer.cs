using UnityEngine;
using System.Collections.Generic;

// Ez az osztály írja le, hogy miből mennyi van a hordóban
[System.Serializable]
public class StoredItem
{
    public Item item;
    public int count;
}

public class StorageContainer : MonoBehaviour
{
    public List<StoredItem> contents = new List<StoredItem>();

    // Beteszünk egy tárgyat
    public void DepositItem(Item itemToSave, int amount)
    {
        // Megnézzük, van-e már ilyen a hordóban
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
                contents.Remove(existing); // Ha elfogyott, töröljük a listából
            }
        }
    }
}