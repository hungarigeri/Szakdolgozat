using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    [System.Serializable]
    public class Slot
    {
        public CollectableType type;
        public int count;
        public int Maxallowed;

        public Sprite icon;

        public Slot()
        {
            type = CollectableType.None;
            count = 0;
            Maxallowed = 99;
        }

        public bool canAddMore()
        {
            if (count < Maxallowed)
            {
                return true;
            }
            return false;
        }

        public void AddItem(Collectable item)
        {
            this.type = item.type;
            this.icon = item.icon;
            this.count += 1;
        }

    }

    public List<Slot> slots = new List<Slot>();

    public Inventory(int numSlots)
    {
        for (int i = 0; i < numSlots; i++)
        {
            Slot slot = new Slot();
            slots.Add(slot);
        }
    }

    public void Add(Collectable item)
    {
        foreach (Slot slot in slots)
        {
            if (slot.type == item.type && slot.canAddMore())
            {
                slot.AddItem(item);
                return;
            }
        }

        foreach (Slot slot in slots)
        {
            if (slot.type == CollectableType.None)
            {
                slot.AddItem(item);
                return;
            }
        }


    }
}
